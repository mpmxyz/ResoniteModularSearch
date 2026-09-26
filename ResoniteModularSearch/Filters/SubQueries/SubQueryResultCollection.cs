using System.Diagnostics.CodeAnalysis;

using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters.SubQueries;
public class SubQueryResultCollection {
    private readonly IDictionary<SubQueryFilterBase, SearchResult> resultPerSubquery = new Dictionary<SubQueryFilterBase, SearchResult>();
    private readonly IList<SubQueryFilterBase> subQueriesInOrder = new List<SubQueryFilterBase>();

    public IEnumerable<KeyValuePair<SubQueryFilterBase, SearchResult>> AllResults {
        get {
            return subQueriesInOrder.Select((subQuery) => KeyValuePair.Create(subQuery, resultPerSubquery[subQuery]));
        }
    }

    public void AddResult(SubQueryFilterBase subQuery, SearchResult result) {
        var alreadyExists = resultPerSubquery.ContainsKey(subQuery);
        resultPerSubquery[subQuery] = result;
        if (!alreadyExists) {
            subQueriesInOrder.Add(subQuery);
        }
    }

    public bool TryGetResult(SubQueryFilterBase subQuery, [MaybeNullWhen(false)] out SearchResult result) {
        return resultPerSubquery.TryGetValue(subQuery, out result);
    }

}
