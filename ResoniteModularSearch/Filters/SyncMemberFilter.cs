using System.Text.RegularExpressions;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Operations;

namespace ResoniteModularSearch.Filters;

[Filter("Sync Member")]
internal class SyncMemberFilter : IFilter {
    private string? _rawNamePattern;

    public string? RawNamePattern { get => _rawNamePattern; set { _rawNamePattern = value; UpdateRegex(); } }

    private Regex? NamePattern { get; set; }

    public string Name => "Sync Member";

    public bool IsValid { get; private set; }

    public Action<IFilterAction>? ApplyAction { private get; set; }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        builder.CreateValueEditor("Name Pattern", RawNamePattern, (value) => RawNamePattern = value);
        builder.NestOut();
    }

    public bool Match(IWorldElement element) {
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

    private void UpdateRegex() {
        NamePattern = TryCompile(RawNamePattern);
        IsValid = NamePattern != null;
    }

    private static Regex? TryCompile(string? source) {
        try {
            return source != null ? new Regex(source) : null;
        } catch {
            return null;
        }
    }
}
