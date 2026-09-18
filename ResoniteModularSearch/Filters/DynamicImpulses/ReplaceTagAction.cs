using FrooxEngine;
using FrooxEngine.Undo;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters.DynamicImpulses;

public class ReplaceTagAction : IFilterAction {
    private readonly DynamicImpulseFilter filter;

    public ReplaceTagAction(DynamicImpulseFilter filter) {
        this.filter = filter;
    }

    public string Name => "Replace tag name";
    public bool IsValid => filter.TagPattern != null && filter.ReplaceTagWith != null;

    public FilterActionResult TryApplyTo(IWorldElement element, SearchContext context) {
        if (!context.Values.TryGetValue<AbstractedDynamicImpulseElement>(element, out var abstractedDynImpulse)) {
            return FilterActionResult.Ignored;
        }
        if (!abstractedDynImpulse.IsValidResult) {
            return FilterActionResult.Ignored;
        }
        var result = FilterActionResult.Ignored;
        foreach (var tagField in abstractedDynImpulse.TagFields) {
            result += RegexHelper.ApplyReplace(tagField.Value,
                                               filter.TagPattern,
                                               filter.ReplaceTagWith,
                                               (value) => {
                                                   if (!DynamicVariableHelper.IsValidName(value)) {
                                                        return FilterActionResult.Failed;
                                                   }
                                                   tagField.CreateUndoPoint();
                                                   tagField.Value = value;
                                                   return FilterActionResult.Success;
                                               });
        }
        return result;
    }
}
