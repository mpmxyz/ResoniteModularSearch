using System.Text.RegularExpressions;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters.DynamicImpulses;

[Filter("Dynamic Impulse")]
public partial class DynamicImpulseFilter : IFilter {
    private static readonly OrderedDictionary<DynamicImpulseElementKind, string> ElementKindFilterDescriptions = new()
    {
        { DynamicImpulseElementKind.ButtonInteraction, "Button interactions" },
        { DynamicImpulseElementKind.ProtoFlux, "ProtoFlux nodes" },
    };
    private static readonly List<PropertySelection.Option<bool?>> IsAsyncOptions = [
        new(null, "Sync or async"),
        new(false, "Sync only"),
        new(true, "Async only"),
    ];
    private static readonly List<PropertySelection.Option<bool?>> WithValueOptions = [
        new(null, "With or without value"),
        new(true, "With value only"),
        new(false, "Without value only"),
    ];
    private static readonly List<PropertySelection.Option<bool?>> IsTriggerOptions = [
        new(null, "Triggers and receivers"),
        new(true, "Triggers only"),
        new(false, "Receivers only"),
    ];
    private static readonly List<PropertySelection.Option<bool?>> IsEnabledOptions = [
        new(null, "Enabled and disabled"),
        new(true, "Enabled only"),
        new(false, "Disabled only"),
    ];

    public ISet<DynamicImpulseElementKind> QueriedKinds { get; set; } = new HashSet<DynamicImpulseElementKind>([DynamicImpulseElementKind.All]);
    public Type? OfType { get; set; } = null;
    public Regex? TagPattern { get; set; } = null;
    public string? ReplaceTagWith { get; set; } = null;
    public bool? IsAsync { get; set; } = null;
    public bool? WithValue { get; set; } = null;
    public bool? IsTrigger { get; set; } = null;
    public bool? IsEnabled { get; set; } = null;

    public string Name => "Dynamic Impulse";

    public bool IsValid => true;

    public Action<IFilterAction>? ApplyAction { get; set; }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        builder.CreateTypeEditor("Of type", OfType, (value) => OfType = value);
        builder.CreateRegexEditor("Tag pattern", TagPattern, (value) => TagPattern = value);
        
        foreach (var (kind, description) in ElementKindFilterDescriptions) {
            builder.CreateValueEditor($"Include {description}", QueriedKinds.Contains(kind), (value) => {
                if (value) {
                    QueriedKinds.Add(kind);
                } else {
                    QueriedKinds.Remove(kind);
                }
            });
        }
        builder.CreateSelection(null, IsAsync, (value) => IsAsync = value, IsAsyncOptions);
        builder.CreateSelection(null, WithValue, (value) => WithValue = value, WithValueOptions);
        builder.CreateSelection(null, IsTrigger, (value) => IsTrigger = value, IsTriggerOptions);
        builder.CreateSelection(null, IsEnabled, (value) => IsEnabled = value, IsEnabledOptions);
        builder.CreateValueEditor("Replace tag with", ReplaceTagWith, (value) => ReplaceTagWith = value);
        ButtonAction.Create(builder, "Replace tag", () => ApplyAction?.Invoke(new ReplaceTagAction(this)));
        builder.NestOut();
    }

    public bool Match(IWorldElement element, SearchContext context) {
        if (!context.TryGetValue<AbstractedDynamicImpulseElement>(element, out var abstractedDynImpulse)) {
            abstractedDynImpulse = DynamicImpulseHelpers.TryGetAbstractedDynamicVariableElement(element);
            context.SetValue(element, abstractedDynImpulse);
        }
        if (!abstractedDynImpulse.IsValidResult) {
            return false;
        }
        if (!QueriedKinds.Contains(abstractedDynImpulse.Kind)) {
            return false;
        }

        if (FailsAsync(abstractedDynImpulse)) {
            return false;
        }
        if (FailsEnabled(abstractedDynImpulse)) {
            return false;
        }
        if (FailsTrigger(abstractedDynImpulse)) {
            return false;
        }
        if (FailsValue(abstractedDynImpulse)) {
            return false;
        }

        if (FailsTag(abstractedDynImpulse)) {
            return false;
        }
        if (FailsType(abstractedDynImpulse.Type)) {
            return false;
        }
        return true;
    }

    private bool FailsAsync(AbstractedDynamicImpulseElement abstractedDynamicImpulse) {
        if (IsAsync.HasValue) {
            if (abstractedDynamicImpulse.IsAsync != IsAsync.Value) {
                return true;
            }
        }
        return false;
    }

    private bool FailsEnabled(AbstractedDynamicImpulseElement abstractedDynamicImpulse) {
        if (IsEnabled.HasValue) {
            if (abstractedDynamicImpulse.IsEnabled != IsEnabled.Value) {
                return true;
            }
        }
        return false;
    }

    private bool FailsTrigger(AbstractedDynamicImpulseElement abstractedDynamicImpulse) {
        if (IsTrigger.HasValue) {
            if (abstractedDynamicImpulse.IsTrigger != IsTrigger.Value) {
                return true;
            }
        }
        return false;
    }

    private bool FailsValue(AbstractedDynamicImpulseElement abstractedDynamicImpulse) {
        if (WithValue.HasValue) {
            if (abstractedDynamicImpulse.WithValue != WithValue.Value) {
                return true;
            }
        }
        return false;
    }

    private bool FailsTag(AbstractedDynamicImpulseElement abstractedDynamicImpulse) {
        if (TagPattern != null) {
            foreach (var tagField in abstractedDynamicImpulse.TagFields) {
                var tag = tagField?.Value;
                if (tag != null && TagPattern.IsMatch(tag)) {
                    return false;
                }
            }
            return true;
        }
        return false;
    }


    private bool FailsType(Type? dynImpulseType) {
        if (OfType != null) {
            if (!OfType.IsEquivalentTo(dynImpulseType)) {
                return true;
            }
        }
        return false;
    }
}
