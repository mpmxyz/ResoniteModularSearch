using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters;

/// <summary>
/// appends all results to a collection (ISyncList with ISyncRef elements excluding ISyncDelegate)
/// </summary>
[Filter("Collect Results")]
public class CollectResultsFilter : IFilter {
    public class CollectAction : IFilterAction {
        private readonly ISyncList? collection;
        private readonly bool allowDuplicates;

        public CollectAction(ISyncList? collection, bool allowDuplicates) {
            this.collection = collection;
            this.allowDuplicates = allowDuplicates;
        }

        public string Name => "Collect";
        public bool IsValid => collection != null;

        public FilterActionResult TryApplyTo(IWorldElement element, SearchContext context) {
            if (collection != null) {
                if (element.IsRemoved) {
                    return FilterActionResult.Failed;
                }
                if (!allowDuplicates) {
                    foreach(var existingSyncMember in collection.Elements) {
                        if (existingSyncMember is ISyncDelegate) {
                            //SyncDelegateLists are not collected to.
                            return FilterActionResult.Failed;
                        }
                        if (existingSyncMember is ISyncRef existingSyncRef) {
                            if (existingSyncRef.Target == element) {
                                //exists already: ignored
                                return FilterActionResult.Ignored;
                            }
                        } else {
                            //The sync collection needs to store references.
                            return FilterActionResult.Failed;
                        }
                    }
                }
                var newEntry = collection.AddElement();
                if (newEntry is ISyncRef syncRef && newEntry is not ISyncDelegate && syncRef.TrySet(element)) {
                    return FilterActionResult.Success;
                } else {
                    //invalid type of element
                    collection.RemoveElement(collection.Count - 1);
                    return FilterActionResult.Failed;
                }
            } else {
                //no collection
                return FilterActionResult.Failed;
            }
        }
    }

    private ISyncList? collection = null;
    public ISyncList? Collection {
        get => collection;
        set {
            if (collection != value) {
                collection = value;
                CollectionChanged?.Invoke();
            }
        }
    }
    private event Action? CollectionChanged = null;

    public string Name => "Collect Results";

    public bool IsValid => true;

    public Action<IFilterAction>? ApplyAction { private get; set; }

    public void Setup(UIBuilder builder) {
        builder.CreateReferenceEditor("SyncElementList", () => Collection, (value) => Collection = value);
        List<ButtonAction> buttons = [
            ButtonAction.Create(builder, "Append all results", () => ApplyAction?.Invoke(new CollectAction(Collection, true))),
            ButtonAction.Create(builder, "Append new results", () => ApplyAction?.Invoke(new CollectAction(Collection, false))),
            ButtonAction.Create(builder, "Clear collection", () => ClearCollection(collection), dangerous: true),
        ];
        void UpdateButtons() {
            try {
                foreach (var button in buttons) {
                    button.Enabled = Collection != null;
                }
            } catch (Exception e) { 
                ResoniteModularSearch.Error($"Exception updating button state: {e}");
            }
        }
        UpdateButtons();
        CollectionChanged += UpdateButtons;
        builder.Current?.Destroyed += (e) => { CollectionChanged -= UpdateButtons; };
    }

    public bool Match(IWorldElement element, SearchContext context) {
        return element is IWorldElement;
    }

    private static void ClearCollection(ISyncList? collection) {
        if (collection != null) {
            int count = collection.Count;
            for (int i = count - 1; i >= 0; i--) {
                collection.RemoveElement(i);
            }
        }
    }
}
