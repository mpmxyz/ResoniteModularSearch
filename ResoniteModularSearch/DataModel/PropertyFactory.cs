using System.Reflection;

using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.DataModel;
public static class PropertyFactory {
    private static FieldInfo typeFieldInfo = typeof(TypeField).GetField(nameof(TypeField.Type))!;
    public static void CreateTypeProperty(this Slot slot, UIBuilder builder, string name, Type initial, Action<Type> onChange) {
        var field = slot.AttachComponent<TypeField>();
        var property = field.Type;
        SyncMemberEditorBuilder.Build(property, name, typeFieldInfo, builder);
        property.Value = initial;
        property.OnValueChange += (e) => {
            if (e.Value != null) {
                onChange(e.Value);
            }
        };
    }
    public static void CreateValueProperty<T>(this Slot slot, UIBuilder builder, string name, T initial, Action<T> onChange) {
        var field = slot.AttachComponent<ValueField<T>>();
        var property = field.Value;
        SyncMemberEditorBuilder.Build(property, name, property.GetType().GetField(nameof(ValueField<T>.Value))!, builder);
        property.Value = initial;
        property.OnValueChange += (e) => {
            if (e.Value != null) {
                onChange(e.Value);
            }
        };
    }

    public static void CreateReferenceProperty<T>(this Slot slot, UIBuilder builder, string name, T? initial, Action<T?> onChange) where T : class, IWorldElement {
        var field = slot.AttachComponent<ReferenceField<T>>();
        var property = field.Reference;
        SyncMemberEditorBuilder.Build(property, name, property.GetType().GetField(nameof(ValueField<T>.Value))!, builder);
#pragma warning disable CS8601 // Possible null reference assignment.
        property.Target = initial;
#pragma warning restore CS8601 // Possible null reference assignment.
        property.OnReferenceChange += (e) => {
            onChange(e.Target);
        };
    }
}
