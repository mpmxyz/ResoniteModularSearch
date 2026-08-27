
using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using HarmonyLib;

using ResoniteModLoader;

namespace ResoniteModularSearch;

public class ResoniteModularSearch : ResoniteMod {
	internal const string VERSION_CONSTANT = "0.0.1";

	public override string Name => "ResoniteModularSearch";
	public override string Author => "mpmxyz";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/mpmxyz/ResoniteModularSearch/";

	public static bool Enabled => enabled.Value;

    public static bool LogExceptions => logExceptions.Value;

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> enabled = new("enabled", "Does the mod do anything?", () => true);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> logExceptions = new("logExceptions", "How to handle errors during replace operations.", () => true);
    public override void OnEngineInit() {
		Harmony harmony = new("mpmxyz.ResoniteModularSearch");
		harmony.PatchAll();
	}

	/// <summary>
	/// This is a minimum example of a mod that adds an action to display "Hello World!" to the DevTool.
	/// </summary>
	[HarmonyPatch(typeof(DevTool), nameof(DevTool.GenerateMenuItems))]
	class DevTool_GenerateMenuItems_Patch {
		public static void Postfix(InteractionHandler tool, ContextMenu menu) {
			if (enabled.Value) {
				menu.AddLocalActionItem("Search", null, colorX.Red, delegate (IButton b, ButtonEventData ev) {
					var slot = b.World.RootSlot.AddSlot("Search Window");
					var builder = new UIBuilder(slot);
					SearchWindow.Create(slot, builder);
				});
			}
		}
	}
}
