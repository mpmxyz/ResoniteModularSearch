using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;

namespace ResoniteModularSearch.Sources;

public class ToParent<T>(IWorldElement searchRoot) : ISearchSource where T : IWorldElement {
    public IWorldElement? SearchRoot { get; set; } = searchRoot;

    public bool IncludeSearchRoot { get; set; } = true;
    public int MaxDepth { get; set; } = -1; //-1 == infinite

    public IEnumerable<IWorldElement> RootElements {
        get {
            if (SearchRoot != null) {
                return [SearchRoot.World.RootSlot];
            } else {
                return [];
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="mask">ignored because it is expected that the search root is not within a "forbidden" hierarchy</param>
    /// <returns></returns>
    public IEnumerable<IWorldElement> GetAllCandidates(Func<IWorldElement, bool> mask) {
        var element = SearchRoot;
        int depth = 0;
        if (!IncludeSearchRoot) {
            if (element is Slot) {
                depth++;
            }
            element = element?.Parent;
        }
        while (element != null) {
            if (depth > MaxDepth && MaxDepth >= 0) {
                break;
            }
            if (element is T) {
                yield return element;
            }
            if (element is Slot) {
                depth++;
            }
            element = element.Parent;
        }
    }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING); //TODO: move vertical layout out of filter UI generation -> use super setup
        builder.CreateReferenceEditor("Search Root", SearchRoot, (value) => SearchRoot = value);
        builder.CreateValueEditor("Max Depth (levels of slots)", MaxDepth, (value) => MaxDepth = value);
        builder.NestOut();
        //TODO: more options?
    }
}
