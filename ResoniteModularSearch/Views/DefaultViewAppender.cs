using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Views;
public class DefaultViewAppender : IResultViewAppender {
    public int AppendOffset { get; set; } = 0;

    public void Setup(UIBuilder builder) {
        throw new NotImplementedException();
    }

    public int TryAppendUI(UIBuilder builder, IWorldElement element, SearchResult result, ResultViewModuleComposition composition) {
        if (composition.TryGetMaskSource(element, result, null, out var _)) {
            return 0;
        }
        int nLines = 0;
        if (element is Worker worker) {
            var uiSlot = builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING).Slot;
            if (element is Slot slot) {
                //TODO: add header
            }
            uiSlot.Name += " (Worker)";
            var workerInspector = uiSlot.AttachComponent<WorkerInspector>();
            builder.PushStyle();
            workerInspector.Setup(worker, (member) => !(element is Slot && member is WorkerBag<Component>) && !composition.TryGetMaskSource(member, result, worker, out var _));
            builder.PopStyle();
            nLines += worker.SyncMemberCount + 1;
            builder.NestOut();
        } else if (element is ISyncMember syncMember) {
            builder.PushStyle();
            builder.Style.MinHeight = StyleHelpers.DEFAULT_MIN_SIZE;
#pragma warning disable CS8604 // Possible null reference argument.
            SyncMemberEditorBuilder.Build(syncMember, syncMember.Name, PropertyAccess.TryGetSyncMemberFieldInfo(syncMember), builder);
#pragma warning restore CS8604 // Possible null reference argument.
            builder.PopStyle();
            nLines++;
        }
        return nLines;
    }
}
