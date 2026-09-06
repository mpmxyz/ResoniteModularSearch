using System.Text.RegularExpressions;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters;

[Filter("Sync Member")]
public class SyncMemberFilter : IFilter {
    public Regex? NamePattern { get; set; }

    public string Name => "Sync Member";

    public bool IsValid { get => NamePattern != null; }

    public Action<IFilterAction>? ApplyAction { private get; set; }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        builder.CreateRegexEditor("Name Pattern", NamePattern, (value) => NamePattern = value);
        builder.NestOut();
    }

    public bool Match(IWorldElement element, SearchContext context) {
        if (element is not ISyncMember member) {
            return false;
        }
        Regex? pattern = NamePattern;
        if (pattern == null) {
            return false;
        }
        string? value = member.Name;
        if (value == null) {
            return false;
        }
        return pattern.IsMatch(value);
    }
}
