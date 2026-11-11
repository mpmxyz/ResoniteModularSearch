using System.Text.RegularExpressions;

using ResoniteModularSearch.FilterActions;

namespace ResoniteModularSearch.DataModel;
public static class RegexHelper {
    public static FilterActionResult ApplyReplace(string? oldValue,
                                                  Regex? searchFor,
                                                  string? replaceWith,
                                                  Func<string, FilterActionResult> setValue) {
        if (oldValue == null) {
            return FilterActionResult.Ignored;
        }
        if (replaceWith == null) {
            return FilterActionResult.Failed;
        }
        string newValue;
        if (searchFor == null) {
            return FilterActionResult.Failed;
        } else {
            newValue = searchFor.Replace(oldValue, replaceWith);
        }
        if (newValue == oldValue) {
            return FilterActionResult.Ignored;
        }
        return setValue(newValue);
    }
    public static FilterActionResult ApplyReplace(string? oldValue,
                                                  Regex? searchFor,
                                                  string? replaceWith,
                                                  Action<string> setValue) {
        return ApplyReplace(oldValue, searchFor, replaceWith, (value) => {
            setValue(value);
            return FilterActionResult.Success;
        });
    }

    public static Regex? TryCompile(string? source) {
        try {
            return source != null ? new Regex(source) : null;
        } catch {
            return null;
        }
    }

    public static Regex? TryCompileExactMatch(string? value) {
        if (value == null) {
            return null;
        }
        return new Regex($"^{Regex.Escape(value)}$");
    }

    public static Regex? TryCompileAnyExactMatch(IEnumerable<string?> values) {
        var alternatives = string.Join("|", values.OfType<string>().Select(Regex.Escape).ToHashSet());
        if (alternatives.Length == 0) {
            return null;
        }
        return new Regex($"^({alternatives})$");
    }
}
