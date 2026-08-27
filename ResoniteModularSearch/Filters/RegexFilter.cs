



using System.Text.RegularExpressions;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Operations;

namespace ResoniteModularSearch.Filters;
internal class RegexFilter : IFilter {
    private string? _rawSearchFor;

    public string? RawSearchFor { get => _rawSearchFor; set { _rawSearchFor = value; UpdateRegex(); } }

    private Regex? SearchFor { get; set; }
    private string? ReplaceWith { get; set; }

    public string Name => "Regular Expression";

    public bool IsValid { get; private set; }

    public List<IFilterAction> FilterActions => [new ReplaceAction(this)];

    public static IFilter Create(Slot slot, UIBuilder builder) {
        var filter = new RegexFilter();
        slot.CreateValueProperty(builder, "Search for", filter.RawSearchFor, (value) => filter.RawSearchFor = value);
        slot.CreateValueProperty(builder, "Replace with", filter.ReplaceWith, (value) => filter.ReplaceWith = value);
        return filter;
    }

    public bool Match(IWorldElement element) {
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

    private void UpdateRegex() {
        SearchFor = TryCompile(RawSearchFor);
        IsValid = SearchFor != null;
    }

    private static Regex? TryCompile(string? source) {
        try {
            return source != null ? new Regex(source) : null;
        } catch {
            return null;
        }
    }

    private string? ReadString(IWorldElement element) {
        if (element is IValue<string> str) {
            return str.Value;
        }
        return null;
    }

    private void WriteString(IWorldElement element, string value) {
        if (element is IValue<string> str) {
            str.Value = value;
        }
    }


    private class ReplaceAction : IFilterAction {
        private readonly RegexFilter filter;

        public ReplaceAction(RegexFilter filter) {
            this.filter = filter;
        }

        public string Name => "Replace";
        public bool IsValid => filter.IsValid && filter.ReplaceWith != null;

        public FilterActionResult TryApplyTo(IWorldElement element) {
            string? oldValue = filter.ReadString(element);
            if (oldValue == null) {
                return FilterActionResult.Ignored;
            }
            Regex? pattern = filter.SearchFor;
            string? replacement = filter.ReplaceWith;
            if (pattern == null || replacement == null) {
                return FilterActionResult.Failed;
            }
            if (!pattern.IsMatch(oldValue)) {
                return FilterActionResult.Ignored;
            }
            //TODO: undo point
            string newValue = pattern.Replace(oldValue, replacement);
            filter.WriteString(element, newValue);
            return FilterActionResult.Success;
        }

    }
}
