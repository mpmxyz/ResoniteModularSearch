using System.Reflection;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.Filters.DynamicVariables;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.DataModel;
public class SearchResultItemView(IWorldElement item, SearchResult result) {

    public IWorldElement Item { get; } = item;
    private SearchResult Result { get; } = result;

    public int TryBuild(UIBuilder builder) {
        UIStyle style = builder.Style.Clone();
        //TODO: selection to manually exclude elements affected by filter action
        var rootSlot = builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING).Slot;
        rootSlot.Name += " (Result)";
        var destroyableParent = Item.FindNearestParent<IDestroyable>();
        if (destroyableParent != null) {
            destroyableParent.Destroyed += (x) => rootSlot.Destroy();
        }
        int nLines = 1;
        {
            if (Item is Slot slot && false) {
                //TODO: special handling?
            } else if (Item is User user && false) {
                //TODO: special handling?
            } else if (Item is Worker worker) {
                var uiSlot = builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING).Slot;
                uiSlot.Name += " (Worker)";
                var workerInspector = uiSlot.AttachComponent<WorkerInspector>();
                workerInspector.Setup(worker);
                nLines += worker.SyncMemberCount;
                //TODO: outsource additional visuals - like dynvars - into its own auto-discovered builder-chain
                //TODO: also allow reduced views of components
                if (Result.Context.TryGetValue<AbstractedDynamicVariableElement>(Item, out var abstractedDynVar)) {
                    if (abstractedDynVar.IsValidResult) {
                        if (abstractedDynVar.NameField != null) {
                            builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING).PaddingLeft.Value = StyleHelpers.DEFAULT_MIN_SIZE;
#pragma warning disable CS8604 // Possible null reference argument.
                            SyncMemberEditorBuilder.Build(abstractedDynVar.NameField, "Name", TryGetSyncMemberFieldInfo(abstractedDynVar.NameField), builder);
#pragma warning restore CS8604 // Possible null reference argument.
                            builder.NestOut();
                        }
                    }
                }

                builder.NestOut();
            } else if (Item is ISyncMember syncMember) {
#pragma warning disable CS8604 // Possible null reference argument.
                SyncMemberEditorBuilder.Build(syncMember, syncMember.Name, TryGetSyncMemberFieldInfo(syncMember), builder);
#pragma warning restore CS8604 // Possible null reference argument.
            }
        }
        builder.NestOut();
        return nLines;

    }

    private static FieldInfo? TryGetSyncMemberFieldInfo(ISyncMember syncMember) {
        if (syncMember.Parent is Worker worker) {
            return worker.GetSyncMemberFieldInfo(syncMember.Name);
        }
        return null;
    }
}
