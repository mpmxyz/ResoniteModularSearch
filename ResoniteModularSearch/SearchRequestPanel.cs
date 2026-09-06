using FrooxEngine;
using FrooxEngine.UIX;
using FrooxEngine.Undo;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Filters;
using ResoniteModularSearch.Search;
using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch;
internal class SearchRequestPanel(ISearchSource source, IFilter filter, Func<IWorldElement, bool> mask) {
    public ISearchSource Source { get; set; } = source;
    public IFilter Filter { get; set; } = filter;

    /// <summary>
    /// This mask is a workaround against search/replace finding and replacing itself.
    /// </summary>
    public Func<IWorldElement, bool> Mask { get; set; } = mask;

    public SearchResult LastResult { get; private set; } = new();
    public event Action<SearchResult>? SearchCompleted;
    public event Action<string> OnStatusChange = ResoniteModularSearch.Msg;
    private World? CurrentWorld { get; set; } = null;

    public static SearchRequestPanel Create(Slot slot, UIBuilder builder, Func<IWorldElement, bool> mask) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING * 3).Slot.Name += " (Search Request Panel)";

        ISearchSource dummySource = FromIWorldElement.Create(slot, builder);
        IFilter rootFilter = new FilterList();

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
            rootFilter.Setup(builder);
            builder.NestOut();
            //builder.NestOut(); Note: ScrollArea + VerticalLayout is only 1 level of nesting!
        }
        SearchRequestPanel panel = new(dummySource, rootFilter, mask);
        rootFilter.ApplyAction = panel.RunAction;
        InfoText? info = null;
        ButtonAction.Create(builder, "Search", panel.RunSearch);
        info = InfoText.Create(builder, "");
        panel.OnStatusChange += (msg) => info.Text = msg;
        panel.CurrentWorld = slot.World;
        builder.NestOut();
        return panel;
    }

    public void RunSearch() {
        var results = SearchAlgorithm.RunSearch(Source, Filter, Mask);
        LastResult = results;
        SearchCompleted?.Invoke(results);
        OnStatusChange($"Found {results.Results.Count} match(es)");
    }

    public void RunAction(IFilterAction action) {
        int nSuccess = 0;
        int nFailed = 0;
        CurrentWorld?.BeginUndoBatch(action.Name);
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
                    ResoniteModularSearch.Error($"Failed to apply action on: {item}\n{e}");
                }
                nFailed++;
            }
        }
        CurrentWorld?.EndUndoBatch();
        OnStatusChange($"{nSuccess} changes, {nFailed} failures");
    }
}
