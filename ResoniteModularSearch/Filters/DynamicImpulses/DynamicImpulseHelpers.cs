using System.Collections.Frozen;
using System.Reflection;

using Elements.Core;

using FrooxEngine;
using FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes;
using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes;

using ProtoFlux.Core;

namespace ResoniteModularSearch.Filters.DynamicImpulses;

/// <summary>
/// Dynamic impulses come in a few flavors:
/// <list type="bullet">
/// <item>trigger and receiver ProtoFlux nodes</item>
/// <item>button triggers</item>
/// <item>both with a payload and without/item>
/// </list>
/// This module tries to mash them together in a way that the differences are less noticable to the user.
/// </summary>
internal static class DynamicImpulseHelpers {
    private static readonly FrozenSet<Type> PlainDynamicVariableBaseClasses =
    [
        typeof(ButtonDynamicImpulseTrigger),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseReceiver),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseTrigger),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseReceiver),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseTrigger),
    ];
    private static readonly FrozenSet<Type> GenericDynamicVariableBaseClasses =
    [
        typeof(ButtonDynamicImpulseTriggerWithValue<>),
        typeof(ButtonDynamicImpulseTriggerWithReference<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseReceiverWithValue<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseReceiverWithObject<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseTriggerWithValue<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseTriggerWithObject<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseReceiverWithValue<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseReceiverWithObject<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseTriggerWithValue<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseTriggerWithObject<>),
    ];

    private static Type? TryGetImpulseType(IWorldElement element) {
        if (element is not Component) {
            //short-circuit impossible combinations -> avoids testing slots and sync members
            return null;
        }
        var elementType = element.GetType();
        while (elementType != null && elementType != typeof(object)) {
            if (elementType.IsGenericType && GenericDynamicVariableBaseClasses.Contains(elementType.GetGenericTypeDefinition())) {
                return elementType.GenericTypeArguments[0];
            }
            elementType = elementType.BaseType;
        }
        return null;
    }
    private static T? TryRunForDynamicImpulse<T>(IWorldElement element, string method, string unmanagedMethod, string worldElementMethod) {
        var type = TryGetImpulseType(element);
        if (type == null) {
            return default;
        }
        if (type.IsUnmanaged()) {
            method = unmanagedMethod;
        } else if (type.IsAssignableTo(typeof(IWorldElement))) {
            method = worldElementMethod;
        }
            return (T?)typeof(DynamicImpulseHelpers).GetGenericMethod(method, BindingFlags.Static | BindingFlags.NonPublic, [type])!.Invoke(null, [element]);
    }
    public static AbstractedDynamicImpulseElement TryGetAbstractedDynamicVariableElement(IWorldElement element) {
        switch(element) {
            case ButtonDynamicImpulseTrigger buttonTrigger:
                return new(kind: DynamicImpulseElementKind.ButtonInteraction,
                           tagFields: new List<IField<string>>([
                                buttonTrigger.HoverEnterTag,
                                buttonTrigger.HoverStayTag,
                                buttonTrigger.HoverLeaveTag,
                                buttonTrigger.PressedTag,
                                buttonTrigger.PressingTag,
                                buttonTrigger.ReleasedTag,
                           ]),
                           isTrigger: true,
                           isAsync: false,
                           isEnabled: buttonTrigger.Enabled);
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseReceiver node:
                return new(kind: DynamicImpulseElementKind.ProtoFlux,
                           tagField: TryGetFieldFromGlobalValue(node.Tag.Target),
                           isTrigger: false,
                           isAsync: false,
                           isEnabled: node.Slot.IsActive && (node.Slot.GetComponent<ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseReceiver.Proxy>()?.Enabled ?? true));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseTrigger node:
                return new(kind: DynamicImpulseElementKind.ProtoFlux,
                           tagField: TryGuessNodeNameField(node.Tag.Target),
                           isTrigger: true,
                           isAsync: false,
                           isEnabled: true);
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseReceiver node:
                return new(kind: DynamicImpulseElementKind.ProtoFlux,
                           tagField: TryGetFieldFromGlobalValue(node.Tag.Target),
                           isTrigger: false,
                           isAsync: true,
                           isEnabled: node.Slot.IsActive && (node.Slot.GetComponent<ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseReceiver.Proxy>()?.Enabled ?? true));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseTrigger node:
                return new(kind: DynamicImpulseElementKind.ProtoFlux,
                           tagField: TryGuessNodeNameField(node.Tag.Target),
                           isTrigger: true,
                           isAsync: true,
                           isEnabled: true);
            default:
                return TryRunForDynamicImpulse<AbstractedDynamicImpulseElement>(
                    element,
                    nameof(TryGetAbstractedDynamicVariableElement),
                    nameof(TryGetAbstractedDynamicVariableElement_Unmanaged),
                    nameof(TryGetAbstractedDynamicVariableElement_IWorldElement)
                ) ?? AbstractedDynamicImpulseElement.InvalidElement;
        }
    }

    private static AbstractedDynamicImpulseElement TryGetAbstractedDynamicVariableElement_Unmanaged<T>(IWorldElement element) where T : unmanaged {
        switch (element) {
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseReceiverWithValue<T> node:
                return new(kind: DynamicImpulseElementKind.ProtoFlux,
                           tagField: TryGetFieldFromGlobalValue(node.Tag.Target),
                           isTrigger: false,
                           isAsync: false,
                           isEnabled: node.Slot.IsActive && ( node.Slot.GetComponent<ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseReceiverWithValue<T>.Proxy>()?.Enabled ?? true),
                           type: typeof(T));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseTriggerWithValue<T> node:
                return new(kind: DynamicImpulseElementKind.ProtoFlux,
                           tagField: TryGuessNodeNameField(node.Tag.Target),
                           isTrigger: true,
                           isAsync: false,
                           isEnabled: true,
                           type: typeof(T));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseReceiverWithValue<T> node:
                return new(kind: DynamicImpulseElementKind.ProtoFlux,
                           tagField: TryGetFieldFromGlobalValue(node.Tag.Target),
                           isTrigger: false,
                           isAsync: true,
                           isEnabled: node.Slot.IsActive && (node.Slot.GetComponent<ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseReceiverWithValue<T>.Proxy>()?.Enabled ?? true),
                           type: typeof(T));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseTriggerWithValue<T> node:
                return new(kind: DynamicImpulseElementKind.ProtoFlux,
                           tagField: TryGuessNodeNameField(node.Tag.Target),
                           isTrigger: true,
                           isAsync: true,
                           isEnabled: true,
                           type: typeof(T));
            default:
                return TryGetAbstractedDynamicVariableElement<T>(element);
        }
    }
    private static AbstractedDynamicImpulseElement TryGetAbstractedDynamicVariableElement_IWorldElement<T>(IWorldElement element) where T : class, IWorldElement {
        switch (element) {
            case ButtonDynamicImpulseTriggerWithReference<T> buttonTrigger:
                return new(kind: DynamicImpulseElementKind.ButtonInteraction,
                           tagFields: new List<IField<string>>([
                                buttonTrigger.HoverEnterData.Tag,
                                buttonTrigger.HoverStayData.Tag,
                                buttonTrigger.HoverLeaveData.Tag,
                                buttonTrigger.PressedData.Tag,
                                buttonTrigger.PressingData.Tag,
                                buttonTrigger.ReleasedData.Tag,
                           ]),
                           isTrigger: true,
                           isAsync: false,
                           isEnabled: buttonTrigger.Enabled,
                           type: typeof(T));
            default:
                return TryGetAbstractedDynamicVariableElement<T>(element);
        }
    }

    private static AbstractedDynamicImpulseElement TryGetAbstractedDynamicVariableElement<T>(IWorldElement element) {
        switch (element) {
            case ButtonDynamicImpulseTriggerWithValue<T> buttonTrigger:
                return new(kind: DynamicImpulseElementKind.ButtonInteraction,
                           tagFields: new List<IField<string>>([
                                buttonTrigger.HoverEnterData.Tag,
                                buttonTrigger.HoverStayData.Tag,
                                buttonTrigger.HoverLeaveData.Tag,
                                buttonTrigger.PressedData.Tag,
                                buttonTrigger.PressingData.Tag,
                                buttonTrigger.ReleasedData.Tag,
                           ]),
                           isTrigger: true,
                           isAsync: false,
                           isEnabled: buttonTrigger.Enabled,
                           type: typeof(T));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseReceiverWithObject<T> node:
                return new(kind: DynamicImpulseElementKind.ProtoFlux,
                           tagField: TryGetFieldFromGlobalValue(node.Tag.Target),
                           isTrigger: false,
                           isAsync: false,
                           isEnabled: node.Slot.IsActive && (node.Slot.GetComponent<ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseReceiverWithObject<T>.Proxy>()?.Enabled ?? true),
                           type: typeof(T));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseTriggerWithObject<T> node:
                return new(kind: DynamicImpulseElementKind.ProtoFlux,
                           tagField: TryGuessNodeNameField(node.Tag.Target),
                           isTrigger: true,
                           isAsync: false,
                           isEnabled: true,
                           type: typeof(T));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseReceiverWithObject<T> node:
                return new(kind: DynamicImpulseElementKind.ProtoFlux,
                           tagField: TryGetFieldFromGlobalValue(node.Tag.Target),
                           isTrigger: false,
                           isAsync: true,
                           isEnabled: node.Slot.IsActive && (node.Slot.GetComponent<ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseReceiverWithObject<T>.Proxy>()?.Enabled ?? true),
                           type: typeof(T));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions.AsyncDynamicImpulseTriggerWithObject<T> node:
                return new(kind: DynamicImpulseElementKind.ProtoFlux,
                           tagField: TryGuessNodeNameField(node.Tag.Target),
                           isTrigger: true,
                           isAsync: true,
                           isEnabled: true,
                           type: typeof(T));
            default:
                throw new NotImplementedException($"Error: Missing implementation to handle type {element.GetType()}");
        }
    }

    private static IField<T>? TryGetFieldFromGlobalValue<T>(IGlobalValueProxy<T> proxy) {
        if (proxy is GlobalValue<T> global) {
            return global.Value;
        }
        return null;
    }
    
    private static Slot? TryGuessNodeSlot(ProtoFluxNode node, ObjectInput<Slot>? target) {
        if (target == null || target?.Source == null) {
            //default behavior of all dynvar nodes with slot input
            return node.Slot;
        }
        //evaluate the input to guess where the dynamic variable space could be
        var group = node.Group;
        if (group != null && group.IsBuilt && group.IsValid) {
            return node.Group.EvaluateImmediatelly((ObjectInput<Slot>)target, default);
        }
        return null;
    }
    private static Slot? TryGuessNodeSlot(ProtoFluxNode node, ObjectArgument<Slot>? target) {
        ObjectInput<Slot>? convertedTarget = target != null ? new() {
            Source = target?.Source
        } : null;
        return TryGuessNodeSlot(node, convertedTarget);
    }
    private static IField<string>? TryGuessNodeNameField(INodeObjectOutput<string>? nameInput) {
        HashSet<INodeObjectOutput<string>> visitedInputs = [];
        while (nameInput != null && visitedInputs.Add(nameInput)) {
            switch(nameInput?.FindNearestParent<Component>()) {
                case ValueObjectInput<string> input:
                    return input.Value;
                case ObjectRelay<string> relay:
                    nameInput = relay.Input.Target;
                    break;
                case ContinuouslyChangingObjectRelay<string> relay:
                    nameInput = relay.Input.Target;
                    break;
                case ObjectValueSource<string> source:
                    return source.Source.Target?.Value as IField<string>;
                default:
                    return null;
            }
        }
        return null;
    }
}
