
using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;
using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch.Filters.SubQueries;

public abstract class SubQueryFilterBase : IFilter {
    public class WrappedAction : IFilterAction {
        private readonly SubQueryFilterBase filter;
        private readonly IFilterAction subQueryAction;

        public WrappedAction(SubQueryFilterBase filter, IFilterAction subqueryAction) {
            this.filter = filter;
            this.subQueryAction = subqueryAction;
        }

        public string Name => subQueryAction.Name;
        public bool IsValid => filter.IsValid && subQueryAction.IsValid;

        public FilterActionResult TryApplyTo(IWorldElement element, SearchContext context) {
            var actionResult = FilterActionResult.Ignored;
            if (context.Values.TryGetValue<SubQueryResultCollection>(element, out var resultCollection)) {
                if (resultCollection.TryGetResult(filter, out var subQueryResult)) {
                    return subQueryResult.RunAction(subQueryAction);
                }
            }
            return actionResult;
        }
    }

    public abstract string Name { get; }

    private FilterList filterList = new();
    public FilterList FilterList {
        get => filterList;
        set {
            filterList.ApplyAction = null;
            filterList = value;
            filterList.ApplyAction = ApplyWrappedAction;
        }
    }
    
    public SubQueryFilterBase() {
        FilterList.ApplyAction = ApplyWrappedAction;
    }

    public bool IsValid => FilterList.IsValid;

    public Action<IFilterAction>? ApplyAction { private get; set; }

    public abstract ISearchSource GetSubQueryElements(IWorldElement element);

    public virtual void Setup(UIBuilder builder) {
        FilterList.Setup(builder);
    }

    public bool Match(IWorldElement element, SearchContext context) {
        var result = context.GetSubQueryContext(FilterList).Search(GetSubQueryElements(element));
        if (result.Results.Count == 0) {
            return false;
        }
        if (!context.Values.TryGetValue<SubQueryResultCollection>(element, out var resultCollection)) {
            resultCollection = new();
            context.Values.SetValue(element, resultCollection);
        }
        resultCollection.AddResult(this, result);
        return true;
    }

    public void ApplyWrappedAction(IFilterAction action) {
        ApplyAction?.Invoke(new WrappedAction(this, action));
    }
}
