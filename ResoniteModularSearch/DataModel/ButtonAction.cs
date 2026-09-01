using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.DataModel;
public class ButtonAction(Button button) {
    private readonly Button button = button;

    public bool Enabled { get => button.Enabled; set => button.Enabled = value; }

    public static ButtonAction Create(UIBuilder builder, string name, Action action) {
        builder.PushStyle();
        builder.Style.Height = StyleHelpers.DEFAULT_MIN_SIZE;
        var button = builder.Button(name);
        var slot = builder.CurrentRect.Slot;
        var toggleField = slot.AttachComponent<ValueField<bool>>();

        button.SetupToggle(toggleField.Value, null, null);
        toggleField.Value.OnValueChange += (v) => action();
        builder.PopStyle();
        return new ButtonAction(button);
    }
}
