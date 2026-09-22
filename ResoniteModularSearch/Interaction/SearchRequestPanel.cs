using FrooxEngine;
using FrooxEngine.UIX;
using FrooxEngine.Undo;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Filters;
using ResoniteModularSearch.Search;
using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch.Interaction;
public class SearchRequestPanel {
    public ISearchSource Source { get; set; }
    public FilterList FilterList { get; set; }

    /// <summary>
    /// This mask is a workaround against search/replace finding and replacing itself.
    /// </summary>
    public Func<IWorldElement, bool> Mask { get; set; }

    public SearchResult LastResult { get; private set; }
    public event Action<SearchResult>? SearchCompleted;
    public event Action<string> OnStatusChange = ResoniteModLoader.ResoniteMod.Msg;
    private World World { get; }

    public SearchRequestPanel(World world, ISearchSource source, FilterList filter, Func<IWorldElement, bool> mask) {
        World = world;
        Source = source;
        FilterList = filter;
        Mask = mask;
        FilterList.ApplyAction = RunAction;
        LastResult = new(new SearchContext(FilterList, Mask));
    }

    public SearchRequestPanel(World world) : this(world, new FromParent<IWorldElement>(world.RootSlot), new FilterList(), (element) => true) {

    }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING * 3).Slot.Name += " (Search Request Panel)";

        Source.Setup(builder);

        builder.PushStyle();
        builder.Style.FlexibleHeight = 1f;
        {
            var scrollArea = builder.ScrollArea();
            /*{
                builder.NestInto(scrollArea.Slot.Parent);
                scrollArea.Slot.Parent.AttachComponent<OverlappingLayout>();
                builder.FitContent(SizeFit.MinSize, SizeFit.Disabled);
                builder.NestOut();
            }*/
            builder.FitContent(SizeFit.MinSize, SizeFit.MinSize);
            builder.PopStyle();
            builder.PushStyle();
            builder.Style.SupressLayoutElement = true;
            builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING); //combined with ScrollArea()
            builder.PopStyle();
            FilterList.Setup(builder);
            builder.NestOut();
            //builder.NestOut(); Note: ScrollArea + VerticalLayout is only 1 level of nesting!
        }
        InfoText? info = null;
        ButtonAction.Create(builder, "Search", RunSearch);
        info = InfoText.Create(builder, "");
        OnStatusChange += (msg) => info.Text = msg;
        builder.NestOut();
    }

    public void RunSearch() {
        var results = new SearchContext(FilterList, Mask).Search(Source);
        LastResult = results;
        SearchCompleted?.Invoke(results);
        OnStatusChange($"Found {results.Results.Count} match(es)");
    }

    public void RunAction(IFilterAction action) {
        World?.BeginUndoBatch(action.Name);
        var result = LastResult.RunAction(action);
        World?.EndUndoBatch();
        OnStatusChange($"{result.SuccessCount} changes, {result.FailureCount} failures");
    }
}
