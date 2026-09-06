using FrooxEngine;
using FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Users;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.DataModel;
public class ButtonAction(Button button) {
    private readonly Button button = button;

    public bool Enabled { get => button.Enabled; set => button.Enabled = value; }

    public static ButtonAction Create(UIBuilder builder, string name, Action<User> action) {
        builder.PushStyle();
        builder.Style.Height = StyleHelpers.DEFAULT_MIN_SIZE;
        var button = builder.Button(name);
        var slot = builder.CurrentRect.Slot;
        var toggleField = slot.AttachComponent<ValueField<bool>>();
        var userField = slot.AttachComponent<ReferenceField<User>>();

        button.SetupToggle(toggleField.Value, null, null);
        var userSet = button.Slot.AttachComponent<ButtonReferenceSet<User>>();
        userSet.TargetReference.Target = userField.Reference;
        var protoFlux = button.Slot.AddSlot("ProtoFlux");
        var localUser = protoFlux.AttachComponent<LocalUser>();
        var drive = protoFlux.AttachComponent<ReferenceDrive<User>>();
        drive.Target.Target = localUser;
        drive.TrySetRootTarget(userSet.SetReference);
        toggleField.Value.OnValueChange += (v) => action(userField.Reference.Target);
        builder.PopStyle();
        return new ButtonAction(button);
    }
    public static ButtonAction Create(UIBuilder builder, string name, Action action) {
        return Create(builder, name, (user) => action());
    }
}
