using System.Diagnostics.CodeAnalysis;

using FrooxEngine;
using FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes;
using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes;

using ProtoFlux.Core;

namespace ResoniteModularSearch.DataModel;
public static class ProtoFluxEvaluation {
    /// <summary>
    /// Tries to evaluate a ProtoFlux node's input.
    /// 
    /// This is not a perfect solution and has many reasons to fail.
    /// One possibility is when the connected output is not connected to a conditionally evaluated input.
    /// The ProtoFlux runtime may not generate an evaluation sequence dedicated to the node.
    /// </summary>
    /// <typeparam name="T">value type expected by the input</typeparam>
    /// <param name="group">node group the node is part of</param>
    /// <param name="input">to be evaluated</param>
    /// <param name="result">contains the computed value if true is returned</param>
    /// <returns>true if the evaluation was successful</returns>
    public static bool TryEvaluate<T>(ProtoFluxNodeGroup group,
                                      ObjectInput<T> input,
                                      [MaybeNullWhen(false)] out T result) {
        if (group.IsBuilt && group.IsValid) {
            try {
                result = group.EvaluateImmediatelly(input, default);
                return true;
            } catch (Exception e) {
                if (ResoniteModularSearch.LogExceptions) {
                    ResoniteModLoader.ResoniteMod.Error($"Failed to evaluate: {input}\n{e}");
                }
            }
        }
        result = default;
        return false;
    }
    /// <summary>
    /// Tries to evaluate a ProtoFlux node's input.
    /// 
    /// This is not a perfect solution and has many reasons to fail.
    /// One possibility is when the connected output is not connected to a conditionally evaluated input.
    /// The ProtoFlux runtime may not generate an evaluation sequence dedicated to the node.
    /// </summary>
    /// <typeparam name="T">value type expected by the input</typeparam>
    /// <param name="group">node group the node is part of</param>
    /// <param name="input">to be evaluated</param>
    /// <param name="result">contains the computed value if true is returned</param>
    /// <returns>true if the evaluation was successful</returns>
    public static bool TryEvaluate<T>(ProtoFluxNodeGroup group,
                                      ProtoFlux.Core.ValueInput<T> input,
                                      [MaybeNullWhen(false)] out T result) where T : unmanaged {
        if (group.IsBuilt && group.IsValid) {
            try {
                result = group.EvaluateImmediatelly(input, default);
                return true;
            } catch (Exception e) {
                if (ResoniteModularSearch.LogExceptions) {
                    ResoniteModLoader.ResoniteMod.Error($"Failed to evaluate: {input}\n{e}");
                }
            }
        }
        result = default;
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="initialOutput"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    public static bool TryFindConstantSyncRef<T>(INodeObjectOutput<T> initialOutput,
                                               [MaybeNullWhen(false)] out ISyncRef<T> field) where T : class, IWorldElement {
        HashSet<INodeObjectOutput<T>> visitedInputs = [];
        INodeObjectOutput<T>? current = initialOutput;
        while (current != null && visitedInputs.Add(current)) {
            switch (current?.FindNearestParent<Component>()) {
                case RefObjectInput<T> input:
                    field = input.Target;
                    return true;
                case ObjectRelay<T> relay:
                    current = relay.Input.Target;
                    break;
                case ContinuouslyChangingObjectRelay<T> relay:
                    current = relay.Input.Target;
                    break;
                case ReferenceSource<T> source:
                    field = source.Source.Target?.Value as ISyncRef<T>;
                    return field != null;
                case SlotRefSource source:
                    field = source.Source.Target?.Value as ISyncRef<T>;
                    return field != null;
                case SlotSource source:
                    field = source.Source.Target?.ValueElement as ISyncRef<T>;
                    return field != null;
                case UserRefSource source:
                    field = source.Source.Target?.Value?.User as ISyncRef<T>;
                    return field != null;
                default:
                    field = null;
                    return false;
            }
        }
        field = null;
        return false;
    }
    public static bool TryFindConstantField<T>(INodeObjectOutput<T> initialOutput,
                                               [MaybeNullWhen(false)] out IField<T> field) {
        HashSet<INodeObjectOutput<T>> visitedInputs = [];
        INodeObjectOutput<T>? current = initialOutput;
        while (current != null && visitedInputs.Add(current)) {
            switch (current?.FindNearestParent<Component>()) {
                case ValueObjectInput<T> input:
                    field = input.Value;
                    return true;
                case TypeObjectInput typeInput:
                    field = typeInput.Type as IField<T>;
                    return field != null;
                case ObjectRelay<T> relay:
                    current = relay.Input.Target;
                    break;
                case ContinuouslyChangingObjectRelay<T> relay:
                    current = relay.Input.Target;
                    break;
                case ObjectValueSource<T> source:
                    field = source.Source.Target?.Value as IField<T>;
                    return field != null;
                default:
                    field = null;
                    return false;
            }
        }
        field = null;
        return false;
    }
    public static bool TryFindConstantField<T>(INodeValueOutput<T> initialOutput, [MaybeNullWhen(false)] out IField<T> field) where T : unmanaged {
        HashSet<INodeValueOutput<T>> visitedInputs = [];
        INodeValueOutput<T>? current = initialOutput;
        while (current != null && visitedInputs.Add(current)) {
            switch (current?.FindNearestParent<Component>()) {
                case FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.ValueInput<T> input:
                    field = input.Value;
                    return true;
                case ValueRelay<T> relay:
                    current = relay.Input.Target;
                    break;
                case ContinuouslyChangingValueRelay<T> relay:
                    current = relay.Input.Target;
                    break;
                case ValueSource<T> source:
                    field = source.Source.Target?.Value as IField<T>;
                    return field != null;
                default:
                    field = null;
                    return false;
            }
        }
        field = null;
        return false;
    }
}
