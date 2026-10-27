using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Search;
using ResoniteModularSearch.Views;

namespace ResoniteModularSearch.Interaction;
public class SearchResultDisplay {
    private static readonly colorX COLOR_EXCLUDE_CHILDREN = colorX.Red.SetValue(ColorHSV.GetValue(colorX.Gray));
    private static readonly colorX COLOR_INCLUDE_CHILDREN = colorX.Green.SetValue(ColorHSV.GetValue(colorX.Gray));
    private static readonly List<PropertySelection.Option<bool>> childDisplayOptions = [
        new(true, "✔ Include Children", COLOR_INCLUDE_CHILDREN),
        new(false, "✘ Exclude Children", COLOR_EXCLUDE_CHILDREN)
    ];

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

    private bool includeChildren = true;
    public bool IncludeChildren {
        get => includeChildren;
        set {
            if (value != includeChildren) {
                includeChildren = value;
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
        Slot hierarchyContent;
        {
            builder.NestInto(columns[0]);
            builder.CurrentRect.OffsetMin.Value = new(StyleHelpers.DEFAULT_SPACING, 0);
            builder.CurrentRect.OffsetMax.Value = new(-StyleHelpers.DEFAULT_SPACING, 0);
            builder.ScrollArea();
            builder.FitContent(SizeFit.MinSize, SizeFit.MinSize);
            {
                hierarchyContent = builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING).Slot;
                builder.NestOut();
            }
            builder.NestOut();
        }
        Slot componentContent;
        PropertyUI selectionUI;
        {
            builder.NestInto(columns[1]);
            builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
            selectionUI = builder.CreateReferenceEditor("Show results within", () => SelectedElement, (newElement) => SelectedElement = newElement);
            builder.CreateSelection(null, () => IncludeChildren, (value) => IncludeChildren = value, childDisplayOptions);
            { 
                builder.HorizontalLayout(StyleHelpers.DEFAULT_SPACING);
                builder.PushStyle();
                builder.Style.FlexibleWidth = 1f;
                ButtonAction.Create(builder, "Select all", () => DisplayedResult?.SetAllSelected(true));
                ButtonAction.Create(builder, "Deselect all", () => DisplayedResult?.SetAllSelected(false));
                builder.PopStyle();
                builder.NestOut();
            }
            {
                builder.PushStyle();
                builder.Style.FlexibleHeight = 1f;
                builder.ScrollArea();
                builder.PopStyle();
                builder.FitContent(SizeFit.Disabled, SizeFit.MinSize);
                {
                    componentContent = builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING).Slot;
                    builder.NestOut();
                }
                builder.NestOut();
            }
            builder.NestOut();
        }
        var styleBase = builder.Style.Clone();
        RebuildHierarchy += () => RebuildHierarchyContent(hierarchyContent, styleBase);
        RebuildResults += () => RebuildResultContent(componentContent, styleBase, selectionUI);
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

    private void RebuildResultContent(Slot componentContentRoot, UIStyle styleBase, PropertyUI selectionUI) {
        var result = DisplayedResult;
        
        componentContentRoot.RunSynchronously(() => {
            selectionUI.Refresh();
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
                if (IsResultItemHidden(item)) {
                    continue;
                }
                if (nDisplayed <= MaxDisplayedResults){
                    nDisplayed += Math.Max(TryDisplayResultItem(builder, item, result), 1);
                } else {
                    nSkipped++;
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

    private bool IsResultItemHidden(IWorldElement resultItem) {
        if (SelectedElement != null && resultItem != SelectedElement) {
            if (IncludeChildren) {
                return !resultItem.IsChildOfElement(SelectedElement);
            } else {
                return resultItem.FindNearestParent<Slot>() != SelectedElement;
            }
        }
        return false;
    }

    private int TryDisplayResultItem(UIBuilder builder, IWorldElement resultItem, SearchResult result) {
        try {
            return ResultViewModuleComposition.Default.BuildResultItemView(builder, resultItem, result);
        } catch (Exception ex) {
            if (ResoniteModularSearch.LogExceptions) {
                ResoniteModLoader.ResoniteMod.Error(ex);
            }
            builder.PushStyle();
            builder.Style.Height = StyleHelpers.DEFAULT_MIN_SIZE;
            builder.Text(ex.Message).Color.Value = colorX.Red;
            builder.PopStyle();
            return 1;
        }
    }
}