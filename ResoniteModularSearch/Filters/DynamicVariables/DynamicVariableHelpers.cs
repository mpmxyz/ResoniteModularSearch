using System.Collections.Frozen;
using System.Reflection;

using Elements.Core;

using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;

using ProtoFlux.Core;

using ResoniteModularSearch.DataModel;

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
        typeof(HoverDynamicValueSet<>),
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
        return (T?)typeof(DynamicVariableHelpers).GetGenericMethod(method, BindingFlags.Static | BindingFlags.NonPublic, [type])!.Invoke(null, [element]);
    }
    public static AbstractedDynamicVariableElement TryGetAbstractedDynamicVariableElement(IWorldElement element) {
        switch (element) {
            case DynamicVariableSpace space:
                return new(kind: DynamicVariableElementKind.Space,
                                   space.SpaceName,
                                   knownSpace: space,
                                   attachedSlot: space?.Slot,
                                   isSpaceOnly: true);
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.ClearDynamicVariables node:
                return new(kind: DynamicVariableElementKind.ProtoFlux,
                                   TryGuessNameField(node.SpaceName.Target),
                                   attachedSlot: TryGuessSearchSlot(node, node.TypedNodeInstance?.Target),
                                   isSpaceOnly: true);
            default:
                return TryRunForDynamicVariable<AbstractedDynamicVariableElement>(element, nameof(TryGetAbstractedDynamicVariableElement))
                            ?? AbstractedDynamicVariableElement.InvalidElement;
        }
    }
    private static AbstractedDynamicVariableElement TryGetAbstractedDynamicVariableElement<T>(IWorldElement element) {
        switch (element) {
            case DynamicVariableBase<T> dynvar:
                return new(kind: DynamicVariableElementKind.Component,
                           nameField: dynvar.VariableName,
                           type: typeof(T),
                           attachedSlot: dynvar.Slot);
            case DynamicVariableResetBase<T> dynvarReset:
                return new(kind: DynamicVariableElementKind.ProtoFlux,
                           nameField: dynvarReset.VariableName,
                           type: typeof(T),
                           attachedSlot: dynvarReset.Slot);
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DynamicVariableInput<T> node:
                return new(kind: DynamicVariableElementKind.ProtoFlux, 
                           nameField: node.VariableName.Target?.ValueElement as IField<string>,
                           type: typeof(T),
                           attachedSlot: node.Slot);
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.ReadDynamicVariable<T> node:
                return new(kind: DynamicVariableElementKind.ProtoFlux,
                           nameField: TryGuessNameField(node.Path.Target),
                           type: typeof(T),
                           attachedSlot: TryGuessSearchSlot(node, (node.NodeInstance as ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.ReadDynamicVariable<T>)?.Source));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.WriteDynamicVariable<T> node:
                return new(kind: DynamicVariableElementKind.ProtoFlux,
                           nameField: TryGuessNameField(node.Path.Target),
                           type: typeof(T),
                           attachedSlot: TryGuessSearchSlot(node, (node.NodeInstance as ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DynamicVariableAction)?.Target));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.CreateDynamicVariable<T> node:
                return new(kind: DynamicVariableElementKind.ProtoFlux,
                           nameField: TryGuessNameField(node.Path.Target),
                           type: typeof(T),
                           attachedSlot: TryGuessSearchSlot(node, (node.NodeInstance as ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DynamicVariableAction)?.Target));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.WriteOrCreateDynamicVariable<T> node:
                return new(kind: DynamicVariableElementKind.ProtoFlux,
                           nameField: TryGuessNameField(node.Path.Target),
                           type: typeof(T),
                           attachedSlot: TryGuessSearchSlot(node, (node.NodeInstance as ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DynamicVariableAction)?.Target));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DeleteDynamicVariable<T> node:
                return new(kind: DynamicVariableElementKind.ProtoFlux,
                           nameField: TryGuessNameField(node.Path.Target),
                           type: typeof(T),
                           attachedSlot: TryGuessSearchSlot(node, (node.NodeInstance as ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DynamicVariableAction)?.Target));
            case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.ClearDynamicVariablesOfType<T> node:
                return new(kind: DynamicVariableElementKind.ProtoFlux,
                           nameField: TryGuessNameField(node.SpaceName.Target),
                           type: typeof(T),
                           attachedSlot: TryGuessSearchSlot(node, (node.NodeInstance as ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Variables.DynamicVariableAction)?.Target),
                           isSpaceOnly: true);
            case HoverDynamicValueSet<T> valueSet:
                return new(kind: DynamicVariableElementKind.ButtonInteraction,
                           nameField: valueSet.VariableName,
                           type: typeof(T),
                           attachedSlot: valueSet.Slot);
            default:
                throw new NotImplementedException($"Error: Missing implementation to handle type {element.GetType()}");
        }
    }

    private static Slot? TryGuessSearchSlot(ProtoFluxNode node, ObjectInput<Slot>? target) {
        if (!target.HasValue || target?.Source == null) {
            //default behavior of all dynvar nodes with slot input
            return node.Slot;
        }
        //evaluate the input to guess where the dynamic variable space could be
        var group = node.Group;
        if (group != null && ProtoFluxEvaluation.TryEvaluate(group, target.Value, out var slot)) {
            return slot;
        }
        return null;
    }
    private static Slot? TryGuessSearchSlot(ProtoFluxNode node, ObjectArgument<Slot>? target) {
        ObjectInput<Slot>? convertedTarget = target != null ? new() {
            Source = target?.Source
        } : null;
        return TryGuessSearchSlot(node, convertedTarget);
    }
    private static IField<string>? TryGuessNameField(INodeObjectOutput<string>? nameInput) {
        if (nameInput != null && ProtoFluxEvaluation.TryFindConstantField(nameInput, out var field)) {
            return field;
        }
        return null;
    }
}
