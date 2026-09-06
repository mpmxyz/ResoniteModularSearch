using FrooxEngine;
using FrooxEngine.Undo;

using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters.DynamicVariables;

public partial class DynamicVariableFilter {
    public class AddSpaceNameAction : IFilterAction {
        private readonly DynamicVariableFilter filter;

        public AddSpaceNameAction(DynamicVariableFilter filter) {
            this.filter = filter;
        }

        public string Name => "Add space name";
        public bool IsValid => true;

        public FilterActionResult TryApplyTo(IWorldElement element, SearchContext context) {
            if (!context.TryGetValue<AbstractedDynamicVariableElement>(element, out var abstractedDynVar)) {
                return FilterActionResult.Ignored;
            }
            if (!abstractedDynVar.IsValidResult) {
                return FilterActionResult.Ignored;
            }
            string? oldValue = abstractedDynVar.SpaceName;
            if (oldValue != null) {
                return FilterActionResult.Ignored;
            }
            DynamicVariableSpace? linkedSpace = abstractedDynVar.Space;
            string? linkedSpaceName = linkedSpace?.SpaceName?.Value;
            if (linkedSpaceName == null) {
                return FilterActionResult.Ignored;
            }
            //in case of directly linking space in between -> do not change
            if (abstractedDynVar.AttachedSlot?.FindSpace(linkedSpaceName) != linkedSpace) {
                return FilterActionResult.Ignored;
            }
            abstractedDynVar.NameField?.CreateUndoPoint();
            abstractedDynVar.SpaceName = linkedSpaceName;
            return FilterActionResult.Success;
        }
    }
}
