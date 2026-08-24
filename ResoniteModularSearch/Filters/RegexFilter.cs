
using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.Operations;

namespace ResoniteModularSearch.Filters;
internal class RegexFilter : IFilter {
    public string Name => "RegexFilter";

    public List<IFilterAction> FilterAction => throw new NotImplementedException();

    public static IFilter Create(Slot slot, UIBuilder builder) {
        throw new NotImplementedException();
    }

    public bool Match(IWorldElement element) {
        throw new NotImplementedException();
    }
}
