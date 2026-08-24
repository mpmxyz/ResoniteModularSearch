
using FrooxEngine;

namespace ResoniteModularSearch.Search;

internal class SearchResult {
    private readonly ISet<IWorldElement> results;
    private readonly Dictionary<IWorldElement, int> recursiveResultCount;
    private readonly Dictionary<Slot, int> directSlotResultCount;
    private readonly Dictionary<User, int> directUserResultCount;

    public SearchResult(ISet<IWorldElement> results) {
        this.results = results;
        recursiveResultCount = [];
        directSlotResultCount = [];
        directUserResultCount = [];

        UpdateResultCounts();
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
