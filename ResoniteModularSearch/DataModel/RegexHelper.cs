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
}
