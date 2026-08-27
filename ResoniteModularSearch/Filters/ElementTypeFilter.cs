using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Operations;

namespace ResoniteModularSearch.Filters;
internal class ElementTypeFilter : IFilter {
    internal Type SearchFor { get; set; } = typeof(object);
    internal bool IncludeSubtypes { get; set; }
    internal bool IsInverted { get; set; }

    public string Name => "Element Type";

    public bool IsValid => SearchFor != null;

    public List<IFilterAction> FilterActions => [];

    public static IFilter Create(Slot slot, UIBuilder builder) {
        var filter = new ElementTypeFilter();
        slot.CreateTypeProperty(builder, "Element Type", filter.SearchFor, (value) => filter.SearchFor = value);
        slot.CreateValueProperty(builder, "Include Subtypes", filter.IncludeSubtypes, (value) => filter.IncludeSubtypes = value);
        slot.CreateValueProperty(builder, "Invert Match", filter.IsInverted, (value) => filter.IsInverted = value);
        return filter;
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
