using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Interaction;
public class SearchResultDisplay {
    private SearchResult? displayedResult = null;
    public SearchResult? DisplayedResult {
        get => displayedResult;
        set {
            displayedResult = value;
            RebuildHierarchy?.Invoke();
            if (SelectedElement != null) {
                SelectedElement = null;
            } else {
                //no change in selection -> force update of display
                RebuildResults?.Invoke();
            }
        }
    }

    private int maxDisplayedResults = 1000;
    public int MaxDisplayedResults {
        get => maxDisplayedResults;
        set {
            maxDisplayedResults = value;
            RebuildResults?.Invoke();
        }
    }

    private IWorldElement? selectedElement = null;
    public IWorldElement? SelectedElement {
        get => selectedElement;
        set {
            if (value != selectedElement) {
                selectedElement = value;
                RebuildResults?.Invoke();
            }
        }
    }

    private event Action? RebuildHierarchy = null;
    private event Action? RebuildResults = null;

    public void Setup(UIBuilder builder) {
        var columns = builder.SplitHorizontally([1,1]);
        var selectionRef = builder.Current.AttachComponent<ReferenceField<IWorldElement>>().Reference;
#pragma warning disable CS8601 // Possible null reference assignment.
        selectionRef.Target = SelectedElement;
#pragma warning restore CS8601 // Possible null reference assignment.
        selectionRef.OnTargetChange += (newSlot) => {
            SelectedElement = newSlot;
        };

        builder.NestInto(columns[0]);
        builder.CurrentRect.OffsetMin.Value = new(StyleHelpers.DEFAULT_SPACING, 0);
        builder.CurrentRect.OffsetMax.Value = new(-StyleHelpers.DEFAULT_SPACING, 0);
        builder.ScrollArea();
        builder.FitContent(SizeFit.MinSize, SizeFit.MinSize);
        var hierarchyContentLayout = builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        var hierarchyContent = hierarchyContentLayout.Slot;
        builder.NestOut();
        builder.NestOut();
        builder.NestInto(columns[1]);
        builder.ScrollArea();
        builder.FitContent(SizeFit.Disabled, SizeFit.MinSize);
        var componentContent = builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING).Slot;
        builder.NestOut();
        builder.NestOut();
        //SearchResultDisplay display = new(hierarchyContent, componentContent, selectionRef, builder.Style.Clone());
        var styleBase = builder.Style.Clone();
        RebuildHierarchy += () => RebuildHierarchyContent(hierarchyContent, styleBase);
        RebuildResults += () => RebuildResultContent(componentContent, styleBase);
        RebuildHierarchy();
        RebuildResults();
    }

    private void RebuildHierarchyContent(Slot hierarchyContentRoot, UIStyle styleBase) {
        var result = DisplayedResult;
        hierarchyContentRoot.RunSynchronously(() => {
            hierarchyContentRoot.DestroyChildren();
            if (result == null) {
                return;
            }

            UIBuilder builder = new(hierarchyContentRoot);
            StyleHelpers.CopyStyleProperties(styleBase, builder.Style);

            foreach (var item in result.Roots) {
                if (item is Slot rootSlot) {
                    SlotHierarchyView view = new(rootSlot, result, (newItem) => SelectedElement = newItem) {
                        Opened = true
                    };
                    view.Setup(builder);
                } else if (item is User user) {
                    //TODO: UserInspectorItem depends on UserInspector which creates an independent tool window
                    //-> custom display
                }
            }
        });
    }

    private void RebuildResultContent(Slot componentContentRoot, UIStyle styleBase) {
        var result = DisplayedResult;
        componentContentRoot.RunSynchronously(() => {
            componentContentRoot.DestroyChildren();
            if (result == null) {
                return;
            }

            UIBuilder builder = new(componentContentRoot);
            StyleHelpers.CopyStyleProperties(styleBase, builder.Style);
            int nDisplayed = 0;
            int nSkipped = 0;
            foreach (var item in result.Results) {
                //TODO: spread UI generation over multiple frames OR make it wait until scrolled enough
                //TODO: option to not show anything with selectedItem==null (if UI generation is not limited)
                //TODO: option to only show elements directly within Slot
                if (SelectedElement == null || item.IsChildOfElement(SelectedElement) || item == SelectedElement) {
                    if (nDisplayed <= MaxDisplayedResults){
                        nDisplayed++;
                        try {
                            nDisplayed += new SearchResultItemView(item, result).TryBuild(builder);
                        } catch (Exception ex) {
                            if (ResoniteModularSearch.LogExceptions) {
                                ResoniteModLoader.ResoniteMod.Error(ex);
                            }
                            builder.PushStyle();
                            builder.Style.Height = StyleHelpers.DEFAULT_MIN_SIZE;
                            builder.Text(ex.Message).Color.Value = colorX.Red;
                            builder.PopStyle();
                        }
                    } else {
                        nSkipped++;
                    }
                }
            }
            if (nSkipped > 0) {
                builder.PushStyle();
                builder.Style.Height = StyleHelpers.DEFAULT_MIN_SIZE;
                builder.Text($"Remaining results without display: {nSkipped}").Color.Value = colorX.Red;
                //TODO: convert to "load more" button
                builder.PopStyle();
            }
        });
    }
}