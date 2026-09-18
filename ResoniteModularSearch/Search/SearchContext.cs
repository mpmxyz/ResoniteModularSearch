using FrooxEngine;

using ResoniteModularSearch.Filters;
using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch.Search;
public class SearchContext {
    private IFilter Filter { get; }
    private Func<IWorldElement, bool> Mask { get; }
    public SearchValues Values { get; private set; } = new();

    private readonly Dictionary<IFilter, SearchContext> subQueryContexts = [];
    private readonly Dictionary<IWorldElement, bool> matchCache = []; //TODO: use cache only for subqueries with cacheable filters

    public SearchContext(IFilter filter, Func<IWorldElement, bool> mask) {
        Filter = filter;
        Mask = mask;
    }

    public SearchResult Search(ISearchSource source) {
        SearchResult result = new(this);
        try {
            foreach (var root in source.RootElements) {
                result.AddRoot(root);
            }
            foreach (var candidate in source.GetAllCandidates(Mask)) {
                if (Match(candidate)) {
                    result.AddResult(candidate);
                }
            }
        } catch (Exception e) {
            if (ResoniteModularSearch.LogExceptions) {
                ResoniteModularSearch.Error($"Exception while running search:\n{e}");
            }
        }
        return result;
    }

    public bool Match(IWorldElement candidate) {
        if (!matchCache.TryGetValue(candidate, out var result))
        {
            result = false;
            try {
                result = Filter.Match(candidate, this);
            } catch (Exception e) {
                if (ResoniteModularSearch.LogExceptions) {
                    ResoniteModularSearch.Error($"Failed to run Match on: {candidate}\n{e}");
                }
            }
            matchCache[candidate] = result;
        }
        return result;
    }

    public SearchContext GetSubQueryContext(IFilter filter) {
        if (!subQueryContexts.TryGetValue(filter, out var subQueryContext)) {
            subQueryContext = new(filter, Mask);
            subQueryContexts[filter] = subQueryContext;
        }
        return subQueryContext;
    }
}
