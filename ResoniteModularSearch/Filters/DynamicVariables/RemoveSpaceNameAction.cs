using FrooxEngine;
using FrooxEngine.Undo;

using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters.DynamicVariables;

public class RemoveSpaceNameAction : IFilterAction {
    private readonly DynamicVariableFilter filter;

    public RemoveSpaceNameAction(DynamicVariableFilter filter) {
        this.filter = filter;
    }

    public string Name => "Remove space name";
    public bool IsValid => true;

    public FilterActionResult TryApplyTo(IWorldElement element, SearchContext context) {
        if (!context.TryGetValue<AbstractedDynamicVariableElement>(element, out var abstractedDynVar)) {
            return FilterActionResult.Ignored;
        }
        if (!abstractedDynVar.IsValidResult) {
            return FilterActionResult.Ignored;
        }
        string? oldValue = abstractedDynVar.SpaceName;
        if (oldValue == null) {
            return FilterActionResult.Ignored;
        }
        DynamicVariableSpace? linkedSpace = abstractedDynVar.Space;
        //in case of space in between -> do not change
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        if (abstractedDynVar.AttachedSlot?.FindSpace(null) != linkedSpace) {
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            return FilterActionResult.Ignored;
        }
        abstractedDynVar.NameField?.CreateUndoPoint();
        abstractedDynVar.SpaceName = null;
        return FilterActionResult.Success;
    }
}
