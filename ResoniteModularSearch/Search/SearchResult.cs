using System.Collections.ObjectModel;

using FrooxEngine;

using ResoniteModularSearch.FilterActions;

namespace ResoniteModularSearch.Search;

public class SearchResult {
    private readonly ISet<IWorldElement> roots;
    private readonly ISet<IWorldElement> results;
    private readonly ISet<IWorldElement> selectedResults;

    private readonly IDictionary<IWorldElement, int> recursiveResultCount;
    private readonly IDictionary<Slot, int> directSlotResultCount;
    private readonly IDictionary<User, int> directUserResultCount;

    public ReadOnlySet<IWorldElement> Roots => roots.AsReadOnly();
    public ReadOnlySet<IWorldElement> Results => results.AsReadOnly();
    public ReadOnlySet<IWorldElement> SelectedResults => selectedResults.AsReadOnly();

    public ReadOnlyDictionary<IWorldElement, int> RecursiveResultCount => recursiveResultCount.AsReadOnly();
    public ReadOnlyDictionary<Slot, int> DirectSlotResultCount => directSlotResultCount.AsReadOnly();
    public ReadOnlyDictionary<User, int> DirectUserResultCount => directUserResultCount.AsReadOnly();

    public SearchContext Context { get; }

    public event Action<IWorldElement>? OnRootAdded;
    public event Action<IWorldElement>? OnResultAdded;
    public event Action<IWorldElement, int>? OnRecursiveResultCountChanged;

    public SearchResult(SearchContext context,
                        ISet<IWorldElement> roots,
                        ISet<IWorldElement> results,
                        ISet<IWorldElement> selectedResults,
                        IDictionary<IWorldElement, int> recursiveResultCount,
                        IDictionary<Slot, int> directSlotResultCount,
                        IDictionary<User, int> directUserResultCount) {
        Context = context;
        this.roots = roots;
        this.results = results;
        this.selectedResults = selectedResults;
        this.recursiveResultCount = recursiveResultCount;
        this.directSlotResultCount = directSlotResultCount;
        this.directUserResultCount = directUserResultCount;
    }

    public SearchResult(SearchContext context) : this(context: context,
                                                      roots: new HashSet<IWorldElement>(),
                                                      results: new HashSet<IWorldElement>(),
                                                      selectedResults: new HashSet<IWorldElement>(),
                                                      recursiveResultCount: new Dictionary<IWorldElement, int>(),
                                                      directSlotResultCount: new Dictionary<Slot, int>(),
                                                      directUserResultCount: new Dictionary<User, int>()) {
    }

    public void AddRoot(IWorldElement root) {
        if (roots.Add(root)) {
            OnRootAdded?.Invoke(root);
        }
    }

    public void AddResult(IWorldElement result) {
        if (!results.Add(result)) {
            return;
        }

        OnResultAdded?.Invoke(result);

        IWorldElement? incrementedItem = result;

        while (incrementedItem != null) {
            var newCount = Increment(recursiveResultCount, incrementedItem);
            OnRecursiveResultCountChanged?.Invoke(incrementedItem, newCount);
            if (incrementedItem is Slot slot) {
                Increment(directSlotResultCount, slot);
                break;
            } else if (incrementedItem is User user) {
                Increment(directUserResultCount, user);
                break;
            }
            incrementedItem = incrementedItem.Parent;
        }
        incrementedItem = incrementedItem?.Parent;
        while (incrementedItem != null) {
            var newCount = Increment(recursiveResultCount, incrementedItem);
            OnRecursiveResultCountChanged?.Invoke(incrementedItem, newCount);
            incrementedItem = incrementedItem.Parent;
        }

        SetSelected(result, true);
    }

    public void SetSelected(IWorldElement result, bool selected) {
        if (!results.Contains(result)) {
            throw new ArgumentException("Can only select elements that are part of the result!", nameof(result));
        }
        if (selected) {
            selectedResults.Add(result);
        } else {
            selectedResults.Remove(result);
        }
        //TODO: run callback if select all/none is to be implemented
    }

    public FilterActionResult RunAction(IFilterAction action) {
        var result = FilterActionResult.Ignored;
        foreach (var item in SelectedResults) {
            try {
                result += action.TryApplyTo(item, Context);
            } catch (Exception e) {
                if (ResoniteModularSearch.LogExceptions) {
                    ResoniteModLoader.ResoniteMod.Error($"Failed to apply action on: {item}\n{e}");
                }
                result += FilterActionResult.Failed;
            }
        }
        return result;
    }

    private static int Increment<K>(IDictionary<K, int> dictionary, K key) {
        int newCount = 1;
        if (dictionary.TryGetValue(key, out var value)) {
            newCount += value;
        }
        dictionary[key] = newCount;
        return newCount;
    }
}
