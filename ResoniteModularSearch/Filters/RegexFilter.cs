



using System.Text.RegularExpressions;

using FrooxEngine;
using FrooxEngine.UIX;
using FrooxEngine.Undo;

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
            return RegexHelper.ApplyReplace(ReadString(element),
                                        filter.SearchFor,
                                        filter.ReplaceWith,
                                        (newValue) => WriteString(element, newValue));

        }
    }

    public Regex? SearchFor { get; set; }
    public string? ReplaceWith { get; set; }

    public string Name => "Regular Expression";

    public bool IsValid { get => SearchFor != null; }

    public Action<IFilterAction>? ApplyAction { private get; set; }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        builder.CreateRegexEditor("Search for", SearchFor, (value) => SearchFor = value);
        builder.CreateValueEditor("Replace with", ReplaceWith, (value) => ReplaceWith = value);
        ButtonAction.Create(builder, "Replace", () => ApplyAction?.Invoke(new ReplaceAction(this)));
        builder.NestOut();
    }

    public bool Match(IWorldElement element, SearchContext context) {
        Regex? pattern = SearchFor;
        if (pattern == null) {
            return false;
        }
        string? value = ReadString(element);
        if (value == null) {
            return false;
        }
        return pattern.IsMatch(value);
    }

    private static string? ReadString(IWorldElement element) {
        if (element is IValue<string> str) {
            return str.Value;
        }
        return null;
    }

    private static void WriteString(IWorldElement element, string value) {
        if (element is IValue<string> str) {
            if (str is IField<string> strField) {
                strField.CreateUndoPoint();
            }
            str.Value = value;
        }
    }
}
