using System.Collections.Frozen;
using System.Reflection;

using Elements.Core;

using FrooxEngine;
using FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes;
using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes;

using ProtoFlux.Core;

namespace ResoniteModularSearch.Filters.DynamicVariables;

/// <summary>
/// Dynamic variables and its tooling come in several flavors:
/// <list type="bullet">
/// <item>simple components based on <see cref="FrooxEngine.DynamicVariableBase"/></item>
/// <item>inheritants of <see cref="FrooxEngine.DynamicVariableResetBase"/></item>
/// <item>ProtoFlux nodes composed of i.e. <see cref="DynamicVariableInput"/> and
/// other components - one of which is the actual dynamic variable</item>
/// </list>
/// This module tries to mash them together in a way that the differences are less noticable to the user.
/// </summary>
internal static class DynamicVariableHelpers {
    private static readonly FrozenSet<Type> PlainDynamicVariableBaseClasses =
    [
        typeof(DynamicVariableSpace),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.ClearDynamicVariables)
    ];
    private static readonly FrozenSet<Type> GenericDynamicVariableBaseClasses =
    [
        typeof(DynamicVariableBase<>),
        typeof(DynamicVariableResetBase<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DynamicVariableInput<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.ReadDynamicVariable<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.WriteDynamicVariable<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.CreateDynamicVariable<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.WriteOrCreateDynamicVariable<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DeleteDynamicVariable<>),
        typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.ClearDynamicVariablesOfType<>),
    ];

    private static Type? TryGetVariableType(IWorldElement element) {
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
    private static T? TryRunForDynamicVariable<T>(IWorldElement element, string method) {
        var type = TryGetVariableType(element);
        if (type == null) {
            return default;
        }
        //TODO: make target method private without breaking GetMethod()
        return (T?)typeof(DynamicVariableHelpers).GetGenericMethod(method, BindingFlags.Static | BindingFlags.NonPublic, [type])!.Invoke(null, [element]);
    }
    public static AbstractedDynamicVariableElement TryGetAbstractedDynamicVariableElement(IWorldElement element) {
        if (element is DynamicVariableSpace space) {
            return new(kind: DynvarElementKind.Space,
                       space.SpaceName,
                       knownSpace: space,
                       attachedSlot: space?.Slot,
                       isSpaceOnly: true);
        } else if (element is FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.ClearDynamicVariables node) {
            return new(kind: DynvarElementKind.ProtoFlux,
                       TryGuessNodeNameField(node.SpaceName.Target),
                       attachedSlot: TryGuessNodeSlot(node, node.TypedNodeInstance?.Target),
                       isSpaceOnly: true);
        } else {
            return TryRunForDynamicVariable<AbstractedDynamicVariableElement>(element, nameof(TryGetAbstractedDynamicVariableElement))
                ?? AbstractedDynamicVariableElement.InvalidElement;
        }
    }
    private static AbstractedDynamicVariableElement TryGetAbstractedDynamicVariableElement<T>(IWorldElement element) {
        switch (element) {
            case DynamicVariableBase<T> dynvar:
                return new(kind: DynvarElementKind.Component,
                           nameField: dynvar.VariableName,
                           type: typeof(T),
                           attachedSlot: dynvar.Slot);
            case DynamicVariableResetBase<T> dynvarReset:
                return new(kind: DynvarElementKind.ProtoFlux,
                           nameField: dynvarReset.VariableName,
                           type: typeof(T),
                           attachedSlot: dynvarReset.Slot);
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DynamicVariableInput<T> node:
                return new(kind: DynvarElementKind.ProtoFlux, 
                           nameField: TryGetFieldFromGlobalValue(node.VariableName.Target),
                           type: typeof(T),
                           attachedSlot: node.Slot);
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.ReadDynamicVariable<T> node:
                return new(kind: DynvarElementKind.ProtoFlux,
                           nameField: TryGuessNodeNameField(node.Path.Target),
                           type: typeof(T),
                           attachedSlot: TryGuessNodeSlot(node, (node.NodeInstance as ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.ReadDynamicVariable<T>)?.Source));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.WriteDynamicVariable<T> node:
                return new(kind: DynvarElementKind.ProtoFlux,
                           nameField: TryGuessNodeNameField(node.Path.Target),
                           type: typeof(T),
                           attachedSlot: TryGuessNodeSlot(node, (node.NodeInstance as ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DynamicVariableAction)?.Target));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.CreateDynamicVariable<T> node:
                return new(kind: DynvarElementKind.ProtoFlux,
                           nameField: TryGuessNodeNameField(node.Path.Target),
                           type: typeof(T),
                           attachedSlot: TryGuessNodeSlot(node, (node.NodeInstance as ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DynamicVariableAction)?.Target));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.WriteOrCreateDynamicVariable<T> node:
                return new(kind: DynvarElementKind.ProtoFlux,
                           nameField: TryGuessNodeNameField(node.Path.Target),
                           type: typeof(T),
                           attachedSlot: TryGuessNodeSlot(node, (node.NodeInstance as ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DynamicVariableAction)?.Target));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DeleteDynamicVariable<T> node:
                return new(kind: DynvarElementKind.ProtoFlux,
                           nameField: TryGuessNodeNameField(node.Path.Target),
                           type: typeof(T),
                           attachedSlot: TryGuessNodeSlot(node, (node.NodeInstance as ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DynamicVariableAction)?.Target));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.ClearDynamicVariablesOfType<T> node:
                return new(kind: DynvarElementKind.ProtoFlux,
                           nameField: TryGuessNodeNameField(node.SpaceName.Target),
                           type: typeof(T),
                           attachedSlot: TryGuessNodeSlot(node, (node.NodeInstance as ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DynamicVariableAction)?.Target),
                           isSpaceOnly: true);
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
