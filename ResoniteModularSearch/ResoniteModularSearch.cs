using System.Reflection;

using Elements.Core;

using FrooxEngine;

using HarmonyLib;

using ResoniteModLoader;

using ResoniteModularSearch.Filters;
using ResoniteModularSearch.Filters.DynamicVariables;
using ResoniteModularSearch.Interaction;
using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch;

public class ResoniteModularSearch : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0";

	public override string Name => "ResoniteModularSearch";
	public override string Author => "mpmxyz";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/mpmxyz/ResoniteModularSearch/";

	public static bool DevToolHasRootSearch => devToolHasRootSearch.Value;
	public static bool DevToolHasSmartSearch => devToolHasSmartSearch.Value;
	public static bool AutoLaunchSmartSearch => autoLaunchSmartSearch.Value;
    public static bool LogExceptions => logExceptions.Value;

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> devToolHasSmartSearch = new("DevToolHasSmartSearch", "Does the dev tool offer a context menu depending on the grabbed item?", () => true);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> devToolHasRootSearch = new("DevToolHasRootSearch", "Does the dev tool offer a search from root without grabbed item?", () => true);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> autoLaunchSmartSearch = new("AutoLaunchSmartSearch", "Does the smart search execute on spawn?", () => true);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> logExceptions = new("Debug.LogExceptions", "Log exceptions thrown by this mod.", () => true);

    public override void OnEngineInit() {
		Harmony harmony = new("mpmxyz.ResoniteModularSearch");
		harmony.PatchAll();
        DevCreateNewForm.AddAction("", "Search & Replace Window", (slot) => {
            new SearchWindow(slot.World).Setup(slot);
        });
	}

	/// <summary>
	/// Adds a menu to the dev tool for search and replace operations
	/// </summary>
	[HarmonyPatch(typeof(DevTool), nameof(DevTool.GenerateMenuItems))]
	class DevTool_GenerateMenuItems_Patch {
		public static void Postfix(InteractionHandler tool, ContextMenu menu) {
            var grabber = tool.Grabber;
            if (grabber == null) {
                return;
            }
            IWorldElement? grabbedReference = null;
            IValueSource? grabbedValueSource = null;
            if (DevToolHasSmartSearch){
                var grabbedReferences = grabber.GrabbedObjects.Select((x) => x.Slot.GetComponent<ReferenceProxy>()?.Reference?.Target).Where((x) => x != null).ToList();
                var grabbedValueSources = grabber.GrabbedObjects.Select((x) => x.Slot.GetComponent<IValueSource>()).Where((x) => x != null).ToList();
                grabbedReference = grabbedReferences.FirstOrDefault();
                grabbedValueSource = grabbedValueSources.FirstOrDefault();
            }

            if (grabbedReference == null && grabbedValueSource == null && DevToolHasRootSearch) {
                grabbedReference = tool.World.RootSlot;
            }

			if (grabbedReference is Slot slot) {
				menu.AddLocalActionItem($"Search from {slot.Name}", null, colorX.Red, delegate (IButton b, ButtonEventData ev) {
					var window = new SearchWindow(b.World);
					var searchRequestPanel = window.SearchRequestPanel;
					searchRequestPanel.Source = new FromParent<IWorldElement>(slot);
					window.Setup(b.World);
				});
			} else if (grabbedReference is DynamicVariableSpace space) {
                menu.AddLocalActionItem($"Search variables in {space.SpaceName.Value}", null, colorX.Red, delegate (IButton b, ButtonEventData ev) {
                    var window = new SearchWindow(b.World);
                    var searchRequestPanel = window.SearchRequestPanel;
                    searchRequestPanel.Source = new FromParent<IWorldElement>(space.Slot);
                    searchRequestPanel.FilterList.AddFilter(new DynamicVariableFilter {
                        WithinSpace = space,
                        QueriedKinds = new HashSet<DynamicVariableElementKind>([
                            DynamicVariableElementKind.Component,
                            DynamicVariableElementKind.ProtoFlux,
                            DynamicVariableElementKind.ButtonInteraction,
                        ])
                    });
                    window.Setup(b.World);
                    if (AutoLaunchSmartSearch) {
                        searchRequestPanel.RunSearch();
                    }
                });
            } else if (grabbedReference != null) {
                menu.AddLocalActionItem($"Search for references to {grabbedReference.Name}", null, colorX.Red, delegate (IButton b, ButtonEventData ev) {
                    var window = new SearchWindow(b.World);
                    var searchRequestPanel = window.SearchRequestPanel;
                    searchRequestPanel.FilterList.AddFilter(new ReferenceFilter {
                        SearchFor = grabbedReference
                    });
                    window.Setup(b.World);
                    if (AutoLaunchSmartSearch) {
                        searchRequestPanel.RunSearch();
                    }
                });
            }
            if (grabbedValueSource != null) {
                var typeArgs = grabbedValueSource.GetType().GetGenericArgumentsFromInterface(typeof(IValueSource<>));
                typeof(ResoniteModularSearch).GetGenericMethod(nameof(TryAddMenuItemForValue), BindingFlags.Static | BindingFlags.NonPublic, typeArgs)!.Invoke(null, [menu, grabbedValueSource]);
            }
        }
    }
    private static void TryAddMenuItemForValue<T>(ContextMenu menu, IValueSource source) {
        if (source is IValueSource<T> valueSource && ValueFilter<T>.IsValidType) {
            menu.AddLocalActionItem($"Search for occurrences of {valueSource.Value}", null, colorX.Red, delegate (IButton b, ButtonEventData ev) {
                var window = new SearchWindow(b.World);
                var searchRequestPanel = window.SearchRequestPanel;
                searchRequestPanel.FilterList.AddFilter(new ValueFilter<T> {
                    SearchFor = valueSource.Value
                });
                window.Setup(b.World);
                if (AutoLaunchSmartSearch) {
                    searchRequestPanel.RunSearch();
                }
            });
        }
    }
}
