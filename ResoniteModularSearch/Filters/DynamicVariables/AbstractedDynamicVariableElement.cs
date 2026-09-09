using FrooxEngine;

namespace ResoniteModularSearch.Filters.DynamicVariables;

internal class AbstractedDynamicVariableElement(DynamicVariableElementKind kind,
                                                IField<string>? nameField,
                                                Type? type = null,
                                                DynamicVariableSpace? knownSpace = null,
                                                Slot? attachedSlot = null,
                                                bool isSpaceOnly = false) {
    public static readonly AbstractedDynamicVariableElement InvalidElement = new(DynamicVariableElementKind.None, null);

    private DynamicVariableSpace? KnownSpace = knownSpace;
    private bool SpaceIsInitialized = knownSpace != null;

    public DynamicVariableElementKind Kind { get; } = kind;
    public Type? Type { get; } = type;
    public Slot? AttachedSlot { get; } = attachedSlot;
    public IField<string>? NameField { get; } = nameField;
    public bool IsSpaceOnly { get; } = isSpaceOnly;
    public bool IsValidResult => Kind != DynamicVariableElementKind.None;

    public DynamicVariableSpace? Space {
        get {
            if (!SpaceIsInitialized) {
                //TODO: cache value for faster lookup in slots with a lot of variables OR do some Harmony magic to get space from dynvar directly
#pragma warning disable CS8604 // Possible null reference argument.
                KnownSpace = AttachedSlot?.FindSpace(SpaceName);
#pragma warning restore CS8604 // Possible null reference argument.
                SpaceIsInitialized = true;
            }
            return KnownSpace;
        }
    }

    public string? SpaceName {
        get {
            if (IsSpaceOnly) {
                return NameField?.Value;
            } else {
#pragma warning disable CS8604 // Possible null reference argument.
                DynamicVariableHelper.ParsePath(NameField?.Value, out string? spaceName, out _);
#pragma warning restore CS8604 // Possible null reference argument.
                return spaceName;
            }
        }
        set {
            if (NameField != null) {
                if (IsSpaceOnly) {
#pragma warning disable CS8601 // Possible null reference assignment.
                    NameField.Value = value;
#pragma warning restore CS8601 // Possible null reference assignment.
                } else {
                    if (value == null) {
#pragma warning disable CS8601 // Possible null reference assignment.
                        NameField.Value = VariableName;
#pragma warning restore CS8601 // Possible null reference assignment.
                    } else {
                        NameField.Value = $"{value}/{VariableName}";
                    }
                }
            }
        }
    }
    public string? VariableName {
        get {
            if (IsSpaceOnly) {
                return null;
            } else {
#pragma warning disable CS8604 // Possible null reference argument.
                DynamicVariableHelper.ParsePath(NameField?.Value, out _, out var varName);
#pragma warning restore CS8604 // Possible null reference argument.
                return varName;
            }
        }
        set {
            if (NameField != null && !IsSpaceOnly) {
                var spaceName = SpaceName;
                if (spaceName == null) {
#pragma warning disable CS8601 // Possible null reference assignment.
                    NameField.Value = value;
#pragma warning restore CS8601 // Possible null reference assignment.
                } else {
                    NameField.Value = $"{spaceName}/{value}";
                }
            }
        }
    }
}