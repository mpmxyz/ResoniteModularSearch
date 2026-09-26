using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.DataModel;
public static class PropertySelection {
    public readonly struct Option<T>(T value, string description, colorX? color = null) {
        public T Value { get; } = value;
        public string Description { get; } = description;
        public colorX? Color { get; } = color;
    }

    public static void CreateSelection<T>(this UIBuilder builder, string? name, Func<T> getter, Action<T> onChange, IEnumerable<Option<T>> options, bool attachArrows = true) {
        var optionList = options.ToList();

        //disconnect inner layouts from whatever happens outside
        builder.PushStyle();
        builder.Style.Height = StyleHelpers.DEFAULT_MIN_SIZE;
        builder.Panel();
        builder.PopStyle();

        builder.PushStyle();
        builder.Style.TextLineHeight = 0.5f;
        builder.Style.TextAutoSizeMin = 16f;
        builder.Style.TextAutoSizeMax = 16f;

        var slot = builder.CurrentRect.Slot;
        var field = slot.AttachComponent<ValueField<int>>();
        var property = field.Value;
        Image labelBackground;
        Text labelText;
        Button labelButton;
        {
            if (attachArrows) {
                builder.PushStyle();
                builder.Style.ForceExpandHeight = true;
                builder.HorizontalLayout(StyleHelpers.DEFAULT_SPACING * 0.5f);
                builder.PopStyle();

                builder.PushStyle();
                builder.Style.Width = StyleHelpers.DEFAULT_MIN_SIZE;
                var prev = BuildShiftButton(builder, "<", property, -1, optionList.Count);
                builder.PopStyle();
            }

            builder.PushStyle();
            builder.Style.Width = 0f;
            builder.Style.FlexibleWidth = 1;
            labelButton = BuildShiftButton(builder, "???", property, +1, optionList.Count);
            labelBackground = labelButton.Slot.GetComponent<Image>();
            labelText = labelButton.Label;
            builder.PopStyle();

            if (attachArrows) {
                builder.PushStyle();
                builder.Style.Width = StyleHelpers.DEFAULT_MIN_SIZE;
                var next = BuildShiftButton(builder, ">", property, +1, optionList.Count);
                builder.PopStyle();

                builder.NestOut();
            }
        }

        builder.PopStyle();
        var defaultTextColor = labelText.Color.Value;
        var defaultBackgroundColor = labelBackground.Tint.Value;
        void UpdateVisuals(int i) {
            var option = optionList[i];
            if (name != null) {
                labelText.Content.Value = $"{name}: {option.Description}";
            } else {
                labelText.Content.Value = option.Description;
            }
            //labelText.Color.Value = defaultTextColor; //TODO
            //This works because InteractionElement.ColorDriver sets up a hook that changes color presets to match what has been written.
            labelBackground.Tint.Value = option.Color ?? defaultBackgroundColor;
        }

        int i = -1;
        field.Value.Value = i;
        var initial = getter();
        foreach (var option in optionList) {
            i++;
            if (object.Equals(option.Value, initial)) {
                UpdateVisuals(i);
                field.Value.Value = i;
            }
        }
        property.OnValueChange += (i) => {
            if (i >= 0 && i < optionList.Count) {
                UpdateVisuals(i);
                onChange(optionList[i].Value);
            }
        };
        builder.NestOut();
    }

    private static Button BuildShiftButton(UIBuilder builder, string text, IField<int> property, int delta, int optionCount) {
        var button = builder.Button(text);
        var label = button.Label;
        label.AlignmentMode.Value = Elements.Assets.AlignmentMode.LineBased;

        var valueShift = builder.Current.AttachComponent<ButtonValueShift<int>>();
        valueShift.Min.Value = 0;
        valueShift.Max.Value = optionCount;
        valueShift.Delta.Value = delta;
        valueShift.MaxIsExclusive.Value = true;
        valueShift.WrapAround.Value = true;
        valueShift.TargetValue.Target = property;

        return button;
    }
}
