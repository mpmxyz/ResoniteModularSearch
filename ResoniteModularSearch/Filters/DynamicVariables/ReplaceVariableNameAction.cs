using System.Text.RegularExpressions;

using FrooxEngine;
using FrooxEngine.Undo;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters.DynamicVariables;

public partial class DynamicVariableFilter {
    public class ReplaceVariableNameAction : IFilterAction {
        private readonly DynamicVariableFilter filter;

        public ReplaceVariableNameAction(DynamicVariableFilter filter) {
            this.filter = filter;
        }

        public string Name => "Replace variable name";
        public bool IsValid => filter.VariableNamePattern != null && filter.ReplaceVariableNameWith != null;

        public FilterActionResult TryApplyTo(IWorldElement element, SearchContext context) {
            if (!context.TryGetValue<AbstractedDynamicVariableElement>(element, out var abstractedDynVar)) {
                return FilterActionResult.Ignored;
            }
            if (!abstractedDynVar.IsValidResult || abstractedDynVar.IsSpaceOnly) {
                return FilterActionResult.Ignored;
            }

            return RegexHelper.ApplyReplace(abstractedDynVar.VariableName,
                                            filter.VariableNamePattern,
                                            filter.ReplaceVariableNameWith,
                                            (value) => {
                                                if (!DynamicVariableHelper.IsValidName(value)) {
                                                    return FilterActionResult.Failed;
                                                }
                                                abstractedDynVar.NameField?.CreateUndoPoint();
                                                abstractedDynVar.VariableName = value;
                                                return FilterActionResult.Success;
                                            });
        }
    }
}
