using System.Text.RegularExpressions;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Operations;

namespace ResoniteModularSearch.Filters;

[Filter("Dynamic Variable")]
internal class DynamicVariableFilter : IFilter {
    internal Type? OfType { get; set; } = null;
    internal DynamicVariableSpace? WithinSpace { get; set; } = null;
    internal Regex? SpaceNamePattern { get; set; } = null;
    internal Regex? VariableNamePattern { get; set; } = null;
    internal bool? IsLinked { get; set; } = null;
    internal string? ReplaceSpaceNameWith { get; set; } = null;
    internal string? ReplaceVariableNameWith { get; set; } = null;

    public string Name => "Dynamic Variable";

    public bool IsValid => true;

    public Action<IFilterAction>? ApplyAction { get; set; }

    private static readonly PropertySelection.Option<bool?>[] LinkOptions =
    [
        new(null, "Linked/Unlinked"),
        new(true, "Linked only"),
        new(false, "Unlinked only"),
    ];
    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        builder.CreateTypeEditor("Of type", OfType, (value) => OfType = value);
        builder.CreateSelection(null, IsLinked, (value) => IsLinked = value, LinkOptions);
        builder.CreateReferenceEditor("Within space", WithinSpace, (value) => WithinSpace = value);
        builder.CreateRegexEditor("Space name pattern", SpaceNamePattern, (value) => SpaceNamePattern = value);
        builder.CreateRegexEditor("Variable name pattern", VariableNamePattern, (value) => VariableNamePattern = value);
        builder.CreateValueEditor("Replace space name with", ReplaceSpaceNameWith, (value) => ReplaceSpaceNameWith = value);
        //TODO: action
        builder.CreateValueEditor("Replace variable name with", ReplaceVariableNameWith, (value) => ReplaceVariableNameWith = value);
        //TODO: action
        builder.NestOut();
    }

    public bool Match(IWorldElement element) {
        //TODO: include dynamic variable spaces in results
        //TODO: custom search context that allows registering values or custom visuals (-> display dynvar inputs as a single entity)
        if (element is not IDynamicVariable dynvar) {
            return false;
        }
        DynamicVariableHelper.ParsePath(dynvar.VariableName, out var spaceName, out var variableName);
        if (SpaceNamePattern != null) {
            if (spaceName == null) {
                return false;
            } else if (!SpaceNamePattern.IsMatch(spaceName)) {
                return false;
            }
        }
        if (VariableNamePattern != null) {
            if (variableName == null) {
                return false;
            } else if (!VariableNamePattern.IsMatch(variableName)) {
                return false;
            }
        }
        if (WithinSpace != null || IsLinked.HasValue) {
            DynamicVariableSpace? space = null;
            //This is not expected to fail:
            if (dynvar.Parent is Slot slot) {
                //The hierarchy check is used to more quickly discard variables that cannot be part of a space
                if (WithinSpace == null || slot.IsChildOf(WithinSpace.Slot, includeSelf: true)){
                    //TODO: cache value for faster lookup in slots with a lot of variables OR do some Harmony magic to get space from dynvar directly
                    space = DynamicVariableHelper.FindSpace(slot, spaceName);
                }
            }
            if (WithinSpace != null) {
                if (WithinSpace != space) {
                    return false;
                }
            }
            if (IsLinked.HasValue) {
                if ((space != null) != IsLinked.Value) {
                    return false;
                }
            }
        }
        if (OfType != null) {
            var expectedType = typeof(IDynamicVariable<>).MakeGenericType(OfType);
            if (!expectedType.IsInstanceOfType(element)) {
                return false;
            }
        }
        return true;
    }
}
