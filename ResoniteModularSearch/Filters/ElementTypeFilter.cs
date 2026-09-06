using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters;

[Filter("Element Type")]
public class ElementTypeFilter : IFilter {
    public Type SearchFor { get; set; } = typeof(object);
    public bool IncludeSubtypes { get; set; } = false;
    public bool IsInverted { get; set; } = false;

    public string Name => "Element Type";

    public bool IsValid => SearchFor != null;

    public Action<IFilterAction>? ApplyAction { get; set; }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        builder.CreateTypeEditor("Element Type", SearchFor, (value) => SearchFor = value ?? SearchFor);
        builder.CreateValueEditor("Include Subtypes", IncludeSubtypes, (value) => IncludeSubtypes = value);
        builder.CreateValueEditor("Invert Match", IsInverted, (value) => IsInverted = value);
        builder.NestOut();
    }

    public bool Match(IWorldElement element, SearchContext context) {
        if (SearchFor != null) {
            bool doesMatch;
            if (IncludeSubtypes) {
                doesMatch = SearchFor.IsInstanceOfType(element);
            } else {
                doesMatch = SearchFor.IsEquivalentTo(element.GetType());
            }
            return doesMatch != IsInverted;
        }
        return false;
    }
}
