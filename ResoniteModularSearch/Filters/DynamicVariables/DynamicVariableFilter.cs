using System.Text.RegularExpressions;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters.DynamicVariables;

[Filter("Dynamic Variable")]
public partial class DynamicVariableFilter : IFilter {
    private static readonly OrderedDictionary<DynvarElementKind, string> ElementKindFilterDescriptions = new()
    {
        { DynvarElementKind.Space, "spaces" },
        { DynvarElementKind.Component, "components" },
        { DynvarElementKind.ProtoFlux, "ProtoFlux nodes" },
    };
    private static readonly PropertySelection.Option<bool?>[] LinkOptions =
    [
        new(null, "Linked/Unlinked"),
        new(true, "Linked only"),
        new(false, "Unlinked only"),
    ];
    private static readonly PropertySelection.Option<bool?>[] MatchLinkedSpaceOptions =
    [
        new(null, "Dynamic"),
        new(true, "Only linked"),
        new(false, "Only defined name"),
    ];

    public ISet<DynvarElementKind> QueriedKinds { get; set; } = new HashSet<DynvarElementKind>([DynvarElementKind.Component]);
    public Type? OfType { get; set; } = null;
    public DynamicVariableSpace? WithinSpace { get; set; } = null;
    public Regex? SpaceNamePattern { get; set; } = null;
    public Regex? VariableNamePattern { get; set; } = null;
    public bool? MatchLinkedSpaceName { get; set; } = null;
    public bool? IsLinked { get; set; } = null;
    public string? ReplaceSpaceNameWith { get; set; } = null;
    public string? ReplaceVariableNameWith { get; set; } = null;

    public string Name => "Dynamic Variable";

    public bool IsValid => true;

    public Action<IFilterAction>? ApplyAction { get; set; }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        builder.CreateTypeEditor("Of type", OfType, (value) => OfType = value);
        builder.CreateSelection("Search for", IsLinked, (value) => IsLinked = value, LinkOptions);
        builder.CreateReferenceEditor("Within space", WithinSpace, (value) => WithinSpace = value);
        builder.CreateRegexEditor("Space name pattern", SpaceNamePattern, (value) => SpaceNamePattern = value);
        builder.CreateSelection("Matched space name", MatchLinkedSpaceName, (value) => MatchLinkedSpaceName = value, MatchLinkedSpaceOptions);
        builder.CreateRegexEditor("Variable name pattern", VariableNamePattern, (value) => VariableNamePattern = value);
        foreach (var (kind, description) in ElementKindFilterDescriptions) {
            builder.CreateValueEditor($"Include {description}", QueriedKinds.Contains(kind), (value) => {
                if (value) {
                    QueriedKinds.Add(kind);
                } else {
                    QueriedKinds.Remove(kind);
                }
            });
        }
        builder.CreateValueEditor("Replace space name with", ReplaceSpaceNameWith, (value) => ReplaceSpaceNameWith = value);
        ButtonAction.Create(builder, "Replace space name", () => ApplyAction?.Invoke(new ReplaceSpaceNameAction(this)));
        builder.CreateValueEditor("Replace variable name with", ReplaceVariableNameWith, (value) => ReplaceVariableNameWith = value);
        ButtonAction.Create(builder, "Replace variable name", () => ApplyAction?.Invoke(new ReplaceVariableNameAction(this)));
        {
            builder.HorizontalLayout(StyleHelpers.DEFAULT_SPACING);
            ButtonAction.Create(builder, "Add space name", () => ApplyAction?.Invoke(new AddSpaceNameAction(this)));
            ButtonAction.Create(builder, "Remove space name", () => ApplyAction?.Invoke(new RemoveSpaceNameAction(this)));
            builder.NestOut();
        }
        builder.NestOut();
    }

    public bool Match(IWorldElement element, SearchContext context) {
        if (!context.TryGetValue<AbstractedDynamicVariableElement>(element, out var abstractedDynVar)) {
            abstractedDynVar = DynamicVariableHelpers.TryGetAbstractedDynamicVariableElement(element);
            context.SetValue(element, abstractedDynVar);
        }
        if (!abstractedDynVar.IsValidResult) {
            return false;
        }
        if (!QueriedKinds.Contains(abstractedDynVar.Kind)) {
            return false;
        }
        
        if (FailsSpaceName(abstractedDynVar)) {
            return false;
        }
        if (!abstractedDynVar.IsSpaceOnly) {
            if (FailsVariableName(abstractedDynVar.VariableName)) {
                return false;
            }
            if (FailsType(abstractedDynVar.Type)) {
                return false;
            }
        }
        if (FailsSpaceLinkage(abstractedDynVar)) {
            return false;
        }
        return true;
    }

    private bool FailsSpaceName(AbstractedDynamicVariableElement abstractedDynVar) {
        if (SpaceNamePattern != null) {
            string? spaceName = MatchLinkedSpaceName switch {
                true => abstractedDynVar.Space?.SpaceName?.Value,
                false => abstractedDynVar.SpaceName,
                _ => abstractedDynVar.SpaceName ?? abstractedDynVar.Space?.SpaceName?.Value
            };
            if (spaceName == null) {
                return true;
            } else if (!SpaceNamePattern.IsMatch(spaceName)) {
                return true;
            }
        }
        return false;
    }

    private bool FailsVariableName(string? variableName) {
        if (VariableNamePattern != null) {
            if (variableName == null) {
                return true;
            } else if (!VariableNamePattern.IsMatch(variableName)) {
                return true;
            }
        }
        return false;
    }

    private bool FailsSpaceLinkage(AbstractedDynamicVariableElement abstractedDynVar) {
        if (WithinSpace != null || IsLinked.HasValue) {
            DynamicVariableSpace? space = null;
            var withinSpaceSlot = WithinSpace?.Slot;
            var dynvarSlot = abstractedDynVar.AttachedSlot;
            if (dynvarSlot != null) {
                //avoid recursive component search if it doesn't make any sense
                if (withinSpaceSlot == null || dynvarSlot.IsChildOf(withinSpaceSlot, includeSelf: true)) {
                    space = abstractedDynVar.Space;
                }
            }
            if (WithinSpace != null) {
                if (WithinSpace != space) {
                    return true;
                }
            }
            if (IsLinked.HasValue) {
                if (space != null != IsLinked.Value) {
                    return true;
                }
            }
        }
        return false;
    }

    private bool FailsType(Type? dynvarType) {
        if (OfType != null) {
            if (!OfType.IsEquivalentTo(dynvarType)) {
                return true;
            }
        }
        return false;
    }
}
