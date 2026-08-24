
using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.Operations;

namespace ResoniteModularSearch.Filters;
internal class ElementTypeFilter : IFilter {
    internal Type? SearchFor { get; set; }
    internal bool IncludeSubtypes { get; set; }
    public bool IsInverted { get; set; }

    public string Name => "Element Type";

    public bool IsValid => SearchFor != null;


    public List<IFilterAction> FilterAction => [];

    public static IFilter Create(Slot slot, UIBuilder builder) {
        throw new NotImplementedException();
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
