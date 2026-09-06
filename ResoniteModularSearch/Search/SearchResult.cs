
using System.Collections.ObjectModel;

using FrooxEngine;

namespace ResoniteModularSearch.Search;

public class SearchResult {
    private readonly ISet<IWorldElement> searchRoots;
    private readonly ISet<IWorldElement> results;
    private readonly Dictionary<IWorldElement, int> recursiveResultCount;
    private readonly Dictionary<Slot, int> directSlotResultCount;
    private readonly Dictionary<User, int> directUserResultCount;
    private readonly SearchContext context;

    public ReadOnlySet<IWorldElement> SearchRoots => searchRoots.AsReadOnly();
    public ReadOnlySet<IWorldElement> Results => results.AsReadOnly();
    public ReadOnlyDictionary<IWorldElement, int> RecursiveResultCount => recursiveResultCount.AsReadOnly();
    public ReadOnlyDictionary<Slot, int> DirectSlotResultCount => directSlotResultCount.AsReadOnly();
    public ReadOnlyDictionary<User, int> DirectUserResultCount => directUserResultCount.AsReadOnly();
    public SearchContext Context => context;

    public SearchResult(ISet<IWorldElement> searchRoots, ISet<IWorldElement> results, SearchContext context) {
        this.searchRoots = searchRoots;
        this.results = results;
        this.context = context.ToFrozenContext();
        recursiveResultCount = [];
        directSlotResultCount = [];
        directUserResultCount = [];

        UpdateResultCounts();
    }

    public SearchResult() : this(new HashSet<IWorldElement>([]), new HashSet<IWorldElement>([]), new SearchContext()) {

    }

    /// <summary>
    /// This recomputes the counts for easy lookup i.e. which slots contain results and should be shown.
    /// Current runtime is O(depth_max * n_results).
    /// </summary>
    private void UpdateResultCounts() {
        recursiveResultCount.Clear();
        directSlotResultCount.Clear();  
        directUserResultCount.Clear();

        foreach (var item in results) {
            IWorldElement? incrementedItem = item;

            while (incrementedItem != null) {
                recursiveResultCount[incrementedItem] = recursiveResultCount.GetValueOrDefault(incrementedItem, 0) + 1;
                if (incrementedItem is Slot slot) {
                    directSlotResultCount[slot] = directSlotResultCount.GetValueOrDefault(slot, 0) + 1;
                    break;
                } else if (incrementedItem is User user) {
                    directUserResultCount[user] = directUserResultCount.GetValueOrDefault(user, 0) + 1;
                    break;
                }
                incrementedItem = incrementedItem.Parent;
            }
            incrementedItem = incrementedItem?.Parent;
            while (incrementedItem != null) {
                recursiveResultCount[incrementedItem] = recursiveResultCount.GetValueOrDefault(incrementedItem, 0) + 1;
                incrementedItem = incrementedItem.Parent;
            }
        }
    }
}
