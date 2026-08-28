
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
	/// Adds a menu to the dev tool for search and replace operations (TODO: move to create new... unless reference is grabbed?)
	/// </summary>
	[HarmonyPatch(typeof(DevTool), nameof(DevTool.GenerateMenuItems))]
	class DevTool_GenerateMenuItems_Patch {
		public static void Postfix(InteractionHandler tool, ContextMenu menu) {
			if (enabled.Value) {
				menu.AddLocalActionItem("Search", null, colorX.Red, delegate (IButton b, ButtonEventData ev) {
                    //TODO: adjust search root based on grabbed references
                    Slot slot = b.World.RootSlot.AddSlot("Search Window");
                    slot.PositionInFrontOfUser(float3.Backward);
                    slot.DestroyWhenUserLeaves(slot.LocalUser);
                    slot.ScaleToUser(slot.LocalUser);
                    SearchWindow.Create(slot);
				});
			}
		}
    }
    /// <summary>
    /// This will hide all UserInspectors without any (child) results that are part of a search display window.
    /// Visible UserInspector's text is updated to show the number of results.
    /// </summary>
    [HarmonyPatch(typeof(SlotInspector), "OnChanges")]
    class SlotInspector_OnChanges_Patch {
        public static void Prefix(SlotInspector __instance) {
            SearchResultDisplay.PropagateSlotInspectorFiltering(__instance);
        }
        public static void Postfix(SlotInspector __instance, SyncRef<Slot> ____rootSlot, SyncRef<Text> ____slotNameText) {
            var rootSlot = ____rootSlot?.Target;
            var displayedText = ____slotNameText.Target?.Content;
            if (rootSlot != null && displayedText != null) {
                SearchResultDisplay.UpdateSlotInspectorVisibilityAndText(__instance, rootSlot, displayedText);
            }
        }
    }
}
