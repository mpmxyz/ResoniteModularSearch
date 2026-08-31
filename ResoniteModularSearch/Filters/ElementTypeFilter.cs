using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Operations;

namespace ResoniteModularSearch.Filters;

[Filter("Element Type")]
internal class ElementTypeFilter : IFilter {
    internal Type SearchFor { get; set; } = typeof(object);
    internal bool IncludeSubtypes { get; set; } = false;
    internal bool IsInverted { get; set; } = false;

    public string Name => "Element Type";

    public bool IsValid => SearchFor != null;

    public Action<IFilterAction>? ApplyAction { get; set; }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        builder.CreateTypeEditor("Element Type", SearchFor, (value) => SearchFor = value);
        builder.CreateValueEditor("Include Subtypes", IncludeSubtypes, (value) => IncludeSubtypes = value);
        builder.CreateValueEditor("Invert Match", IsInverted, (value) => IsInverted = value);
        builder.NestOut();
    }

    public bool Match(IWorldElement element) {
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
