using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Operations;

namespace ResoniteModularSearch.Filters;
internal class ElementTypeFilter : IFilter {
    internal Type SearchFor { get; set; } = typeof(object);
    internal bool IncludeSubtypes { get; set; } = false;
    internal bool IsInverted { get; set; } = false;

    public string Name => "Element Type";

    public bool IsValid => SearchFor != null;

    public List<IFilterAction> FilterActions => [];

    public void Setup(Slot slot, UIBuilder builder) {
        slot.CreateTypeProperty(builder, "Element Type", SearchFor, (value) => SearchFor = value);
        slot.CreateValueProperty(builder, "Include Subtypes", IncludeSubtypes, (value) => IncludeSubtypes = value);
        slot.CreateValueProperty(builder, "Invert Match", IsInverted, (value) => IsInverted = value);
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
