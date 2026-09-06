using FrooxEngine;

using ResoniteModularSearch.Filters;
using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch.Search;
internal class SearchAlgorithm {
    public static SearchResult RunSearch(ISearchSource source, IFilter query, Func<IWorldElement, bool> mask) {
        SearchContext context = new();
        HashSet<IWorldElement> matchingItems=[];
        IEnumerable<IWorldElement> rootElements = source.RootElements;
        InternalRunSearch(rootElements, query, mask, matchingItems, context);
        return new(new HashSet<IWorldElement>(rootElements), matchingItems, context.ToFrozenContext());
    }

    private static void InternalRunSearch(IEnumerable<IWorldElement> items, IFilter query, Func<IWorldElement, bool> mask, HashSet<IWorldElement> matchingItems, SearchContext context) {
        foreach (var item in items) {
            if (mask(item)) {
                try {
                    if (query.Match(item, context)) {
                        matchingItems.Add(item);
                    }
                } catch (Exception e) {
                    if (ResoniteModularSearch.LogExceptions) {
                        ResoniteModularSearch.Error($"Failed to run Match on: {item}\n{e}");
                    }
                }

                //TODO: a way to skip parts of the recursion (i.e. no iteration over properties if filter only matches slots)
                //Idea: add flag to query that specifies a filter to GetChildren
                InternalRunSearch(GetChildren(item), query, mask, matchingItems, context);
            }
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
