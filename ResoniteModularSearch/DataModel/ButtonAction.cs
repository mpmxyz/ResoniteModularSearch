using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.DataModel;
public class ButtonAction(Button button) {
    private readonly Button button = button;

    public bool Enabled { get => button.Enabled; set => button.Enabled = value; }

    public static ButtonAction Create(Slot data, UIBuilder builder, string name, Action action) {
        var button = builder.Button(name);
        var toggleField = data.AttachComponent<ValueField<bool>>();

        button.SetupToggle(toggleField.Value, null, null);
        toggleField.Value.OnValueChange += (v) => action();
        
        return new ButtonAction(button);
    }
}
