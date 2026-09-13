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

    public SearchResult LastResult { get; private set; } = new();
    public event Action<SearchResult>? SearchCompleted;
    public event Action<string> OnStatusChange = ResoniteModLoader.ResoniteMod.Msg;
    private World World { get; }

    public SearchRequestPanel(World world, ISearchSource source, FilterList filter, Func<IWorldElement, bool> mask) {
        World = world;
        Source = source;
        FilterList = filter;
        Mask = mask;
        FilterList.ApplyAction = RunAction;
    }

    public SearchRequestPanel(World world) : this(world, new FromIWorldElement(world.RootSlot), new FilterList(), (element) => true) {

    }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING * 3).Slot.Name += " (Search Request Panel)";

        Source.Setup(builder);

        builder.PushStyle();
        builder.Style.FlexibleHeight = 1f;
        {
            builder.ScrollArea();
            builder.FitContent(SizeFit.Disabled, SizeFit.MinSize);
            builder.PopStyle();
            builder.PushStyle();
            builder.Style.SupressLayoutElement = true;
            builder.VerticalLayout(); //combined with ScrollArea()
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
        var results = SearchAlgorithm.RunSearch(Source, FilterList, Mask);
        LastResult = results;
        SearchCompleted?.Invoke(results);
        OnStatusChange($"Found {results.Results.Count} match(es)");
    }

    public void RunAction(IFilterAction action) {
        int nSuccess = 0;
        int nFailed = 0;
        World?.BeginUndoBatch(action.Name);
        foreach (var item in LastResult.Results) {
            try {
                switch (action.TryApplyTo(item, LastResult.Context)) {
                    case FilterActionResult.Success:
                        nSuccess++;
                        break;
                    case FilterActionResult.Failed:
                        nFailed++;
                        break;
                }
            } catch (Exception e) {
                if (ResoniteModularSearch.LogExceptions) {
                    ResoniteModLoader.ResoniteMod.Error($"Failed to apply action on: {item}\n{e}");
                }
                nFailed++;
            }
        }
        World?.EndUndoBatch();
        OnStatusChange($"{nSuccess} changes, {nFailed} failures");
    }
}
