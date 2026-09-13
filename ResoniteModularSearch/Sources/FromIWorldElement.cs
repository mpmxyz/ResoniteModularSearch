
using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;

namespace ResoniteModularSearch.Sources;

//TODO: change to multiple search roots
public class FromIWorldElement(IWorldElement searchRoot) : ISearchSource {
    internal IWorldElement? SearchRoot { get; set; } = searchRoot;

    public IEnumerable<IWorldElement> RootElements => SearchRoot != null ? [SearchRoot] : [];

    public void Setup(UIBuilder builder) {
        builder.CreateReferenceEditor("Search Root", SearchRoot, (value) => SearchRoot = value);
    }
}
