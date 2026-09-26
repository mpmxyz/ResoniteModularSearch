using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Search;
using ResoniteModularSearch.Views;

namespace ResoniteModularSearch.Filters.SubQueries;
public class SubQueryViewModule : IResultViewAppender {
    public int AppendOffset { get; set; } = +20;

    public void Setup(UIBuilder builder) {
        throw new NotImplementedException();
    }

    public int TryAppendUI(UIBuilder builder, IWorldElement element, SearchResult result, ResultViewModuleComposition composition) {
        int nLines = 0;
        if (result.Context.Values.TryGetValue<SubQueryResultCollection>(element, out var resultCollection)) {
            builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
            foreach (var (subQuery, subQueryResult) in resultCollection.AllResults) {
                //TODO: Button to show results on demand only (next to header)
                //TODO: better way to differentiate filters of equal type
                builder.PushStyle();
                builder.Style.Height = StyleHelpers.DEFAULT_MIN_SIZE;
                builder.Text($"<u>{subQuery.Name}</u>");
                builder.PopStyle();
                var container = builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
                container.PaddingLeft.Value = StyleHelpers.DEFAULT_SPACING;
                //TODO: merge code with list of results from SearchResultDisplay
                foreach (var resultElement in subQueryResult.Results) {
                    nLines += composition.BuildResultItemView(builder, resultElement, subQueryResult);
                }
                builder.NestOut();
            }
            builder.NestOut();
        }
        return nLines;
    }
}
