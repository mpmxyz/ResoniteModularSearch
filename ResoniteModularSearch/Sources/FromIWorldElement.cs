
using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;

namespace ResoniteModularSearch.Sources;
internal class FromIWorldElement : ISearchSource {
    internal IWorldElement? SearchRoot { get; set; }

    public static ISearchSource Create(Slot slot, UIBuilder builder) {
        FromIWorldElement source = new();
        source.SearchRoot = slot.World.RootSlot;
        builder.CreateReferenceProperty("Search Root", source.SearchRoot, (value) => source.SearchRoot = value);
        return source;
    }

    public IEnumerable<IWorldElement> RootElements => SearchRoot != null ? [SearchRoot] : [];
}
