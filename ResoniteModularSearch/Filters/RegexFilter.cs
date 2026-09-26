



using System.Text.RegularExpressions;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters;

[Filter("Regular Expression")]
public class RegexFilter : IFilter {
    public class ReplaceAction : IFilterAction {
        private readonly RegexFilter filter;

        public ReplaceAction(RegexFilter filter) {
            this.filter = filter;
        }

        public string Name => "Replace";
        public bool IsValid => filter.IsValid && filter.ReplaceWith != null;

        public FilterActionResult TryApplyTo(IWorldElement element, SearchContext context) {
            Regex? searchFor = filter.SearchFor;
            string? replaceWith = filter.ReplaceWith;
            if (searchFor == null || replaceWith == null) {
                return FilterActionResult.Failed;
            }
            PropertyAccess.PropertyStatistics result = PropertyAccess.UndoableEditAllMatchingValues<string>(
                                                element: element,
                                                //This does not match using the regex as non-matching values will be ignored anyway.
                                                //(Replace causes no change -> ignored)
                                                predicate: (value) => value != null,
                                                transform: (value) => searchFor.Replace(value, replaceWith));
            if (result.TotalCount == 0) {
                return FilterActionResult.Ignored;
            } else {
                return FilterActionResult.Success;
            }
        }
    }

    public Regex? SearchFor { get; set; }
    public string? ReplaceWith { get; set; }

    public string Name => "Regular Expression";

    public bool IsValid { get => SearchFor != null; }

    public Action<IFilterAction>? ApplyAction { private get; set; }

    public void Setup(UIBuilder builder) {
        builder.CreateRegexEditor("Search for", () => SearchFor, (value) => SearchFor = value);
        builder.CreateValueEditor("Replace with", () => ReplaceWith, (value) => ReplaceWith = value);
        ButtonAction.Create(builder, "Replace", () => ApplyAction?.Invoke(new ReplaceAction(this)));
    }

    public bool Match(IWorldElement element, SearchContext context) {
        Regex? searchFor = SearchFor;
        if (searchFor == null) {
            return false;
        }
        PropertyAccess.PropertyStatistics result = PropertyAccess.CountMatchingValues<string>(element, searchFor.IsMatch);
        return result.TotalCount != 0;
    }
}
