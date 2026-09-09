using FrooxEngine;

namespace ResoniteModularSearch.Filters.DynamicImpulses;

internal class AbstractedDynamicImpulseElement(DynamicImpulseElementKind kind,
                                               IList<IField<string>> tagFields,
                                               bool isTrigger,
                                               bool isAsync,
                                               bool isEnabled,
                                               Type? type = null) {
    public static readonly AbstractedDynamicImpulseElement InvalidElement = new(DynamicImpulseElementKind.None, [], false, false, false);

    public AbstractedDynamicImpulseElement(DynamicImpulseElementKind kind,
                                               IField<string>? tagField,
                                               bool isTrigger,
                                               bool isAsync,
                                               bool isEnabled,
                                               Type? type = null) : this(kind, tagField != null ? [tagField]: [], isTrigger, isAsync, isEnabled, type) {

    }

    public DynamicImpulseElementKind Kind { get; } = kind;
    public IList<IField<string>> TagFields { get; } = tagFields;
    public bool IsEnabled { get; } = isEnabled;
    public bool IsTrigger { get; } = isTrigger;
    public bool IsAsync { get; } = isAsync;
    public Type? Type { get; } = type;
    public bool WithValue => Type != null;
    public bool IsValidResult => Kind != DynamicImpulseElementKind.None;
}