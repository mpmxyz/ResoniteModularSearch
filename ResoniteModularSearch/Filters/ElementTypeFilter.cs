using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters;

[Filter("Is Type")]
public class ElementTypeFilter : IFilter {
    public Type SearchFor { get; set; } = typeof(object);
    public bool IncludeSubtypes { get; set; } = false;

    public string Name => "Is Type";

    public bool IsValid => SearchFor != null;

    public Action<IFilterAction>? ApplyAction { get; set; }

    public void Setup(UIBuilder builder) {
        builder.CreateTypeEditor("Type", () => SearchFor, (value) => SearchFor = value ?? SearchFor);
        builder.CreateValueEditor("Include Subtypes", () => IncludeSubtypes, (value) => IncludeSubtypes = value);
    }

    public bool Match(IWorldElement element, SearchContext context) {
        if (SearchFor != null) {
            if (IncludeSubtypes) {
                return SearchFor.IsInstanceOfType(element);
            } else {
                return SearchFor.IsEquivalentTo(element.GetType());
            }
        }
        return false;
    }
}
