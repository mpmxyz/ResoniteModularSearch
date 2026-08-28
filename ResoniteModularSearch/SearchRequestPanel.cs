using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Filters;
using ResoniteModularSearch.Operations;
using ResoniteModularSearch.Search;
using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch;
internal class SearchRequestPanel(ISearchSource source, IFilter filter, Func<IWorldElement, bool> mask) {
    public ISearchSource Source { get; set; } = source;
    public IFilter Filter { get; set; } = filter;
    public Func<IWorldElement, bool> Mask { get; set; } = mask;

    public SearchResult LastResult { get; private set; } = new SearchResult();
    public event Action<SearchResult>? SearchCompleted;
    public event Action<string> OnStatusChange=ResoniteModularSearch.Msg;

    public static SearchRequestPanel Create(Slot slot, UIBuilder builder, Func<IWorldElement, bool> mask) {
        builder.VerticalLayout();

        ISearchSource dummySource = FromIWorldElement.Create(slot, builder);
        IFilter dummyFilter = new RegexFilter();
        dummyFilter.Setup(slot, builder);

        SearchRequestPanel panel = new(dummySource, dummyFilter, mask);
        InfoText? info = null;
        foreach (var action in dummyFilter.FilterActions) {
            ButtonAction.Create(slot, builder, action.Name, () => panel.RunAction(action));
        }
        ButtonAction.Create(slot, builder, "Search", panel.RunSearch);
        info = InfoText.Create(slot, builder, "");
        panel.OnStatusChange += (msg) => info.Text = msg;
        return panel;
    }

    public void RunSearch() {
        var results = SearchAlgorithm.RunSearch(Source, Filter, Mask);
        LastResult = results;
        SearchCompleted?.Invoke(results);
        OnStatusChange($"Found {results.Results.Count} match(es)");
    }

    public void RunAction(IFilterAction action) {
        //TODO - bug: search/replace config replaces itself
        int nSuccess = 0;
        int nFailed = 0;
        foreach (var item in LastResult.Results) {
            switch (action.TryApplyTo(item)) {
                case FilterActionResult.Success:
                    nSuccess++;
                    break;
                case FilterActionResult.Failed:
                    nFailed++;
                    break;
            }
        }
        OnStatusChange($"{nSuccess} changes, {nFailed} failures");
    }
}
