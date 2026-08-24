using FrooxEngine;

using ResoniteModularSearch.Filters;
using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch.Search;
internal class SearchAlgorithm {
    public static SearchResult RunSearch(ISearchSource source, IFilter query) {
        HashSet<IWorldElement> matchingItems=[];
        IEnumerable<IWorldElement> rootElements = source.RootElements;
        InternalRunSearch(rootElements, query, matchingItems);
        return new(matchingItems);
    }

    private static void InternalRunSearch(IEnumerable<IWorldElement> items, IFilter query, HashSet<IWorldElement> matchingItems) {
        foreach (var item in items) {
            if (query.Match(item)) {
                matchingItems.Add(item);
            }
            //TODO: a way to skip parts of the recursion (i.e. no iteration over properties if filter only matches slots)
            //Idea: add flag to query that specifies a filter to GetChildren
            InternalRunSearch(GetChildren(item), query, matchingItems);
        }
    }

    private static IEnumerable<IWorldElement> GetChildren(IWorldElement parent) {
        if (parent is Worker worker) {
            foreach (var child in worker.GetSyncMembers<IWorldElement>()) {
                yield return child;
            }
            if (parent is Slot slot) {
                foreach (var component in slot.Components) {
                    yield return component;
                }
                foreach (var child in slot.Children) {
                    yield return child;
                }
            } else if (parent is User user) {
                foreach (var component in user.Components) {
                    yield return component;
                }
            }
        }
    }
}
