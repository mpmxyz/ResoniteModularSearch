using System.Reflection;
using System.Text.RegularExpressions;

using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.DataModel;
public static class PropertyEditor {
    private static readonly FieldInfo typeFieldInfo = typeof(TypeField).GetField(nameof(TypeField.Type))!;
    
    //TODO: add validation callback
    public static PropertyUI CreateTypeEditor(this UIBuilder builder, string name, Func<Type?> getter, Action<Type?> onChange) {
        builder.PushStyle();
        builder.Style.Height = StyleHelpers.DEFAULT_MIN_SIZE;
        var slot = builder.CurrentRect.Slot;
        var field = slot.AttachComponent<TypeField>();
        var property = field.Type;
        SyncMemberEditorBuilder.Build(property, name, typeFieldInfo, builder);
        void OnRefresh() {
#pragma warning disable CS8601 // Possible null reference assignment.
            property.Value = getter();
#pragma warning restore CS8601 // Possible null reference assignment.
        }
        OnRefresh();
        property.OnValueChange += (e) => {
            onChange(e.Value);
        };
        builder.PopStyle();
        return new PropertyUI(OnRefresh);
    }

    public static PropertyUI CreateValueEditor<T>(this UIBuilder builder, string name, Func<T> getter, Action<T> onChange) {
        builder.PushStyle();
        builder.Style.Height = StyleHelpers.DEFAULT_MIN_SIZE;
        var slot = builder.CurrentRect.Slot;
        var field = slot.AttachComponent<ValueField<T>>();
        var property = field.Value;
        SyncMemberEditorBuilder.Build(property, name, property.GetType().GetField(nameof(ValueField<T>.Value))!, builder);
        void OnRefresh(){
            property.Value = getter();
        }
        OnRefresh();
        property.OnValueChange += (e) => {
            onChange(e.Value);
        };
        builder.PopStyle();
        return new PropertyUI(OnRefresh);
    }

    public static PropertyUI CreateReferenceEditor<T>(this UIBuilder builder, string name, Func<T?> getter, Action<T?> onChange) where T : class, IWorldElement {
        builder.PushStyle();
        builder.Style.Height = StyleHelpers.DEFAULT_MIN_SIZE;
        var slot = builder.CurrentRect.Slot;
        var field = slot.AttachComponent<ReferenceField<T>>();
        var property = field.Reference;
        SyncMemberEditorBuilder.Build(property, name, property.GetType().GetField(nameof(ValueField<T>.Value))!, builder);
        void OnRefresh(){
#pragma warning disable CS8601 // Possible null reference assignment.
            property.Target = getter();
#pragma warning restore CS8601 // Possible null reference assignment.
        }
        OnRefresh();
        property.OnTargetChange += (e) => {
            onChange(e.Target);
        };
        builder.PopStyle();
        return new PropertyUI(OnRefresh);
    }

    public static PropertyUI CreateRegexEditor(this UIBuilder builder, string name, Func<Regex?> getter, Action<Regex?> onChange) {
        //TODO: highlight if input contains invalid Regex
        return CreateValueEditor(builder, name, () => getter()?.ToString(), value => onChange(TryCompile(value)));
    }

    private static Regex? TryCompile(string? source) {
        try {
            return source != null ? new Regex(source) : null;
        } catch {
            return null;
        }
    }
}
