
using System.Reflection;
using System.Runtime.CompilerServices;

using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch;
public class SearchResultDisplay {
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
        builder.FitContent(SizeFit.MinSize, SizeFit.MinSize);
        var hierarchyContentLayout = builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        hierarchyContentLayout.PaddingLeft.Value = StyleHelpers.DEFAULT_SPACING;
        hierarchyContentLayout.PaddingRight.Value = StyleHelpers.DEFAULT_SPACING;
        var hierarchyContent = hierarchyContentLayout.Slot;
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
            UIBuilder builder = new(HierarchyContentRoot);
            StyleHelpers.CopyStyleProperties(Style, builder.Style);

            foreach (var item in result.SearchRoots) {
                if (item is Slot rootSlot) {
                    SlotHierarchyView view = new(rootSlot, DisplayedResult) {
                        Opened = true
                    };
                    view.TryBuild(builder, SelectedElementRef);
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