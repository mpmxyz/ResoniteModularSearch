using FrooxEngine;
using FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Users;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.DataModel;
public class ButtonAction(Button button) {
    private readonly Button button = button;

    public bool Enabled { get => button.Enabled; set => button.Enabled = value; }

    public static ButtonAction Create(Button button, Action<User> action, bool dangerous = false, Action<bool>? onLockChanged = null) {
        var slot = button.Slot;
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

        var unlocked = !dangerous;
        onLockChanged?.Invoke(unlocked);
        if (dangerous) {
            button.IsHovering.OnValueChange += (hovering) => {
                if (dangerous && !hovering && unlocked) {
                    unlocked = false;
                    onLockChanged?.Invoke(unlocked);
                }
            };
        }

        toggleField.Value.OnValueChange += (v) => {
            if (unlocked) {
                action(userField.Reference.Target);
                if (dangerous) {
                    unlocked = false;
                    onLockChanged?.Invoke(unlocked);
                }
            } else {
                unlocked = true;
                onLockChanged?.Invoke(unlocked);
            }
        };
        return new ButtonAction(button);

    }

    public static ButtonAction Create(UIBuilder builder, string name, Action<User> action, bool dangerous = false) {
        builder.PushStyle();
        builder.Style.Height = StyleHelpers.DEFAULT_MIN_SIZE;
        var button = builder.Button(name);

        void UpdateButtonLabel(bool unlocked) {
            button.LabelText = dangerous && unlocked ? $"<color=\"red\">{name}?</color>" : name;
        }
        var buttonAction = Create(button, action, dangerous, UpdateButtonLabel);
        builder.PopStyle();
        return buttonAction;
    }
    public static ButtonAction Create(UIBuilder builder, string name, Action action, bool dangerous = false) {
        return Create(builder, name, (user) => action(), dangerous);
    }
}
