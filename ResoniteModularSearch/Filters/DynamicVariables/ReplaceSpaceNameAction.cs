using System.Text.RegularExpressions;

using FrooxEngine;
using FrooxEngine.Undo;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters.DynamicVariables;

public partial class DynamicVariableFilter {
    public class ReplaceSpaceNameAction : IFilterAction {
        private readonly DynamicVariableFilter filter;

        public ReplaceSpaceNameAction(DynamicVariableFilter filter) {
            this.filter = filter;
        }

        public string Name => "Replace space name";
        public bool IsValid => filter.SpaceNamePattern != null && filter.ReplaceSpaceNameWith != null;

        public FilterActionResult TryApplyTo(IWorldElement element, SearchContext context) {
            if (!context.TryGetValue<AbstractedDynamicVariableElement>(element, out var abstractedDynVar)) {
                return FilterActionResult.Ignored;
            }
            if (!abstractedDynVar.IsValidResult) {
                return FilterActionResult.Ignored;
            }

            return RegexHelper.ApplyReplace(abstractedDynVar.SpaceName,
                                            filter.SpaceNamePattern,
                                            filter.ReplaceSpaceNameWith,
                                            (value) => {
                                                if (!DynamicVariableHelper.IsValidName(value)) {
                                                    return FilterActionResult.Failed;
                                                }
                                                abstractedDynVar.NameField?.CreateUndoPoint();
                                                abstractedDynVar.SpaceName = value;
                                                return FilterActionResult.Success;
                                            });
        }
    }
}
