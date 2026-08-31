
using System.Reflection;
using System.Runtime.CompilerServices;

using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch;
public class SearchResultDisplay {
    private static ConditionalWeakTable<SlotInspector, SearchResultDisplay> FilteredSlotInspectors { get; } = [];

    private static SearchResultDisplay? GetAndPropagateSearchDisplay(SlotInspector inspector) {
        //ResoniteModularSearch.Msg($"GetAndPropagateSearchResult: {inspector.ReferenceID}");
        SearchResultDisplay? display;
        if (!FilteredSlotInspectors.TryGetValue(inspector, out display)) {
            //ResoniteModularSearch.Msg($"No result yet");
            //no result, try parent (assumes the first calls for parents happen before the first calls to their children)
            var parentInspector = inspector.Slot.GetComponentInParents<SlotInspector>(includeSelf: false);
            if (parentInspector == null) {
                //ResoniteModularSearch.Msg($"No parent");
                //not part of a search result (root)
                return null;
            }
            //ResoniteModularSearch.Msg($"Parent: {parentInspector.ReferenceID}");
            if (!FilteredSlotInspectors.TryGetValue(parentInspector, out display)) {
                //ResoniteModularSearch.Msg($"Parent is not part of search display.");
                //not part of a search result (non-root)
                return null;
            }
            //ResoniteModularSearch.Msg($"Adding...");
            //new child, add to table
            FilteredSlotInspectors.AddOrUpdate(inspector, display);
        }
        return display;
    }

    /// <summary>
    /// This method needs to be called before child inspectors are created. (i.e. Prefix of OnChanges)
    /// </summary>
    /// <param name="inspector"></param>
    public static void PropagateSlotInspectorFiltering(SlotInspector inspector) {
        GetAndPropagateSearchDisplay(inspector);
    }

    public static void UpdateSlotInspectorVisibilityAndText(SlotInspector inspector, Slot inspectedSlot, IValue<string> displayedText) {
        SearchResultDisplay? display = GetAndPropagateSearchDisplay(inspector);
        if (display == null) {
            return;
        }
        //Replace slot record by button action to set filter element (needs to be postfix of OnChanges)
        //TODO: move to patch code
        var record = inspector.Slot.GetComponentInChildren<SlotRecord>(excludeDisabled: true);
        if (record != null) {
            var recordButtonSlot = record.Slot;
            var referenceSet = recordButtonSlot.AttachComponent<ButtonReferenceSet<IWorldElement>>();
            referenceSet.TargetReference.Target = display.SelectedElementRef;
            referenceSet.SetReference.Target = record.TargetSlot.Target;
            //TODO: support grabbing the reference
            record.Destroy();
        }
        var recursiveCount = display.DisplayedResult.RecursiveResultCount.GetValueOrDefault(inspectedSlot, 0);
        var directCount = display.DisplayedResult.DirectSlotResultCount.GetValueOrDefault(inspectedSlot, 0);
        inspector.Slot.ActiveSelf = recursiveCount > 0;
        displayedText.Value = $"({directCount}/{recursiveCount}) {inspectedSlot.Name}";
    }

    private SearchResult displayedResult = new();
    public SearchResult DisplayedResult {
        get => displayedResult;
        set {
            displayedResult = value;
            UpdateHierarchy();
            if (SelectedElementRef.Target != null) {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                SelectedElementRef.Target = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            } else {
                //no change in selection -> force update of display
                UpdateSelection();
            }
        }
    }

    private int maxDisplayedResults = 1000;
    public int MaxDisplayedResults {
        get => maxDisplayedResults;
        set {
            maxDisplayedResults = value;
            UpdateSelection();
        }
    }

    public IWorldElement? SelectedElement {
        get => SelectedElementRef.Target;
        set {
#pragma warning disable CS8601 // Possible null reference assignment.
            SelectedElementRef.Target = value as Slot;
#pragma warning restore CS8601 // Possible null reference assignment.
        }
    }

    private Slot HierarchyContentRoot { get; }
    private Slot ComponentContentRoot { get; }
    private SyncRef<IWorldElement> SelectedElementRef { get; }
    private UIStyle Style { get; }

    public SearchResultDisplay(Slot hierarchyContentRoot, Slot componentContentRoot, SyncRef<IWorldElement> selectionRef, UIStyle style) {
        HierarchyContentRoot = hierarchyContentRoot;
        ComponentContentRoot = componentContentRoot;
        SelectedElementRef = selectionRef;
        Style = style;
        SelectedElementRef.OnTargetChange += (newSlot) => {
            UpdateSelection();
        };
    }

    public static SearchResultDisplay Create(Slot slot, UIBuilder builder) {
        var columns = builder.SplitHorizontally([1,1]);
        builder.NestInto(columns[0]);
        builder.ScrollArea();
        builder.FitContent(SizeFit.Disabled, SizeFit.MinSize);
        var hierarchyContent = builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING).Slot;
        builder.NestOut();
        builder.NestOut();
        builder.NestInto(columns[1]);
        builder.ScrollArea();
        builder.FitContent(SizeFit.Disabled, SizeFit.MinSize);
        var componentContent = builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING).Slot;
        builder.NestOut();
        builder.NestOut();
        var selectionRef = slot.AttachComponent<ReferenceField<IWorldElement>>().Reference;
        SearchResultDisplay display = new(hierarchyContent, componentContent, selectionRef, builder.Style.Clone());
        return display;
    }

    private void UpdateHierarchy() {
        var result = DisplayedResult;
        HierarchyContentRoot.RunSynchronously(() => {
            HierarchyContentRoot.DestroyChildren();
            foreach (var item in result.SearchRoots) {
                if (item is Slot rootSlot) {
                    var inspector = HierarchyContentRoot.AddSlot("Filtered Slot Hierarchy").AttachComponent<SlotInspector>();
                    FilteredSlotInspectors.AddOrUpdate(inspector, this);
                    var slotSelectionRef = HierarchyContentRoot.AttachComponent<ReferenceField<Slot>>().Reference;
                    var cast = HierarchyContentRoot.AttachComponent<ReferenceCast<IWorldElement, Slot>>();
                    cast.Source.Target = SelectedElementRef;
                    cast.Target.Target = slotSelectionRef;
                    cast.WriteBack.Value = true;
                    inspector.Setup(rootSlot, slotSelectionRef);
                } else if (item is User user) {
                    //TODO: UserInspectorItem depends on UserInspector which creates an independent tool window
                    //-> custom display
                }
            }
        });
    }

    private void UpdateSelection() {
        var result = DisplayedResult;
        ComponentContentRoot.RunSynchronously(() => {
            ComponentContentRoot.DestroyChildren();
            UIBuilder builder = new(ComponentContentRoot);
            StyleHelpers.CopyStyleProperties(Style, builder.Style);
            int nDisplayed = 0;
            foreach (var item in DisplayedResult.Results) {
                //TODO: spread UI generation over multiple frames OR make it wait until scrolled enough
                //TODO: option to not show anything with selectedItem==null (if UI generation is not limited)
                //TODO: option to only show elements directly within Slot
                if (SelectedElement == null || item.IsChildOfElement(SelectedElement) || item == SelectedElement) {
                    if (nDisplayed <= MaxDisplayedResults){
                        nDisplayed++;
                        try {
                            if (item is Slot slot && false) {
                                //TODO
                            } else if (item is User user && false) {
                                //TODO
                            } else if (item is Worker worker && false) {
                                //TODO
                            } else if (item is ISyncMember syncMember) {
#pragma warning disable CS8604 // Possible null reference argument.
                                SyncMemberEditorBuilder.Build(syncMember, syncMember.Name, TryGetSyncMemberFieldInfo(syncMember), builder);
#pragma warning restore CS8604 // Possible null reference argument.
                            }
                        } catch (Exception ex) {
                            if (ResoniteModularSearch.LogExceptions) {
                                ResoniteModularSearch.Error(ex);
                            }
                            builder.Text(ex.Message).Color.Value = colorX.Red;
                        }
                    }
                }
            }
            //TODO add info if more results are available
        });
    }

    private static FieldInfo? TryGetSyncMemberFieldInfo(ISyncMember syncMember) {
        if (syncMember.Parent is Worker worker) {
            return worker.GetSyncMemberFieldInfo(syncMember.Name);
        }
        return null;
    }
}