using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Filters.DynamicVariables;
using ResoniteModularSearch.Search;
using ResoniteModularSearch.Views;

namespace ResoniteModularSearch.Filters.DynamicImpulses;
public class DynamicImpulseViewModule : IResultViewAppender, IResultViewMask {
    public int AppendOffset { get; set; } = +10;
    public int MaskOffset { get; set; } = +10;

    public bool MasksElement(IWorldElement element, SearchResult result, IWorker? partOf) {
        if (partOf != null) {
            if (result.Context.Values.TryGetValue<AbstractedDynamicVariableElement>(element, out var abstractedDynVar)) {
                switch (element.Name) {
                    case "UpdateOrder":
                        return true;
                    case "Enabled":
                        return true;
                    case "Persistent":
                        //TODO: option
                        return true;
                    case "Tag":
                        return true;
                }
            }
        }
        return false;
    }

    public int TryAppendUI(UIBuilder builder, IWorldElement element, SearchResult result, ResultViewModuleComposition composition) {
        int nLines = 0;
        if (result.Context.Values.TryGetValue<AbstractedDynamicImpulseElement>(element, out var abstractedDynImpulse)) {
            if (abstractedDynImpulse.IsValidResult) {
                if (abstractedDynImpulse.TagFields.Count > 0) {
                    builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING).PaddingLeft.Value = StyleHelpers.DEFAULT_MIN_SIZE;
                    builder.PushStyle();
                    builder.Style.MinHeight = StyleHelpers.DEFAULT_MIN_SIZE;
                    foreach (var field in abstractedDynImpulse.TagFields) {
#pragma warning disable CS8604 // Possible null reference argument.
                        SyncMemberEditorBuilder.Build(field, "Tag", PropertyAccess.TryGetSyncMemberFieldInfo(field), builder);
#pragma warning restore CS8604 // Possible null reference argument.
                        nLines++;
                    }
                    builder.PopStyle();
                    builder.NestOut();
                }
            }
        }
        return nLines;
    }

    public void Setup(UIBuilder builder) {
        throw new NotImplementedException();
    }
}
