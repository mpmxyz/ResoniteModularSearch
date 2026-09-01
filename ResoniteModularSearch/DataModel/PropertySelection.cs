using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.DataModel;
public static class PropertySelection {
    public readonly struct Option<T>(T value, string description) {
        public T Value { get; } = value;
        public string Description { get; } = description;
    }

    public static void CreateSelection<T>(this UIBuilder builder, string? name, T initial, Action<T> onChange, IEnumerable<Option<T>> options) {
        var optionList = options.ToList();
        
        builder.PushStyle();
        builder.Style.Height = StyleHelpers.DEFAULT_MIN_SIZE;
        var slot = builder.CurrentRect.Slot;
        var field = slot.AttachComponent<ValueField<int>>();
        var property = field.Value;

        var button = builder.Button($"{name}");
        var label = button.Slot.GetComponentInChildren<Text>();
        void UpdateText(int i) {
            if (name != null) {
                label.Content.Value = $"{name}: {optionList[i].Description}";
            } else {
                label.Content.Value = optionList[i].Description;
            }
        }
        var valueShift = builder.Current.AttachComponent<ButtonValueShift<int>>();
        valueShift.Min.Value = 0;
        valueShift.Max.Value = optionList.Count;
        valueShift.Delta.Value = 1;
        valueShift.MaxIsExclusive.Value = true;
        valueShift.WrapAround.Value = true;
        valueShift.TargetValue.Target = property;

        int i = -1;
        field.Value.Value = i;
        foreach (var option in optionList) {
            i++;
            if (object.Equals(option.Value, initial)) {
                UpdateText(i);
                field.Value.Value = i;
            }
        }
        property.OnValueChange += (i) => {
            if (i >= 0 && i < optionList.Count) {
                UpdateText(i);
                onChange(optionList[i].Value);
            }
        };
        builder.PopStyle();
    }
}
