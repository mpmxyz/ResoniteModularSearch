using System.Reflection;

using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.DataModel;
public static class PropertyEditor {
    private static FieldInfo typeFieldInfo = typeof(TypeField).GetField(nameof(TypeField.Type))!;
    public static void CreateTypeEditor(this UIBuilder builder, string name, Type initial, Action<Type?> onChange) {
        builder.PushStyle();
        builder.Style.Height = StyleHelpers.DEFAULT_HEIGHT;
        var slot = builder.CurrentRect.Slot;
        var field = slot.AttachComponent<TypeField>();
        var property = field.Type;
        SyncMemberEditorBuilder.Build(property, name, typeFieldInfo, builder);
        property.Value = initial;
        property.OnValueChange += (e) => {
            onChange(e.Value);
        };
        builder.PopStyle();
    }

    public static void CreateValueEditor<T>(this UIBuilder builder, string name, T initial, Action<T> onChange) {
        builder.PushStyle();
        builder.Style.Height = StyleHelpers.DEFAULT_HEIGHT;
        var slot = builder.CurrentRect.Slot;
        var field = slot.AttachComponent<ValueField<T>>();
        var property = field.Value;
        SyncMemberEditorBuilder.Build(property, name, property.GetType().GetField(nameof(ValueField<T>.Value))!, builder);
        property.Value = initial;
        property.OnValueChange += (e) => {
            onChange(e.Value);
        };
        builder.PopStyle();
    }

    public static void CreateReferenceEditor<T>(this UIBuilder builder, string name, T? initial, Action<T?> onChange) where T : class, IWorldElement {
        builder.PushStyle();
        builder.Style.Height = StyleHelpers.DEFAULT_HEIGHT;
        var slot = builder.CurrentRect.Slot;
        var field = slot.AttachComponent<ReferenceField<T>>();
        var property = field.Reference;
        SyncMemberEditorBuilder.Build(property, name, property.GetType().GetField(nameof(ValueField<T>.Value))!, builder);
#pragma warning disable CS8601 // Possible null reference assignment.
        property.Target = initial;
#pragma warning restore CS8601 // Possible null reference assignment.
        property.OnTargetChange += (e) => {
            onChange(e.Target);
        };
        builder.PopStyle();
    }
}
