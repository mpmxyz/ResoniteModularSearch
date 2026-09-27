using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Search;
using ResoniteModularSearch.Views;

namespace ResoniteModularSearch.Filters.DynamicVariables;
public class DynamicVariableViewModule : IResultViewAppender, IResultViewMask {
    public int AppendOffset { get; set; } = +10;
    public int MaskOffset { get; set; } = +10;

    public bool MasksElement(IWorldElement element, SearchResult result, IWorker? partOf) {
        if (partOf != null) {
            if (result.Context.Values.TryGetValue<AbstractedDynamicVariableElement>(element, out var abstractedDynVar)) {
                switch(element.Name) {
                    case "UpdateOrder":
                        return true;
                    case "Enabled":
                        return true;
                    case "Persistent":
                        //TODO: option
                        return true;
                    case "VariableName":
                        return true;
                    case "SpaceName":
                        return true;
                }
            }
        }
        return false;
    }

    public int TryAppendUI(UIBuilder builder, IWorldElement element, SearchResult result, ResultViewModuleComposition composition) {
        int nLines = 0;
        if (result.Context.Values.TryGetValue<AbstractedDynamicVariableElement>(element, out var abstractedDynVar)) {
            if (abstractedDynVar.IsValidResult) {
                if (abstractedDynVar.NameField != null) {
                    builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING).PaddingLeft.Value = StyleHelpers.DEFAULT_MIN_SIZE;
                    builder.PushStyle();
                    builder.Style.MinHeight = StyleHelpers.DEFAULT_MIN_SIZE;
#pragma warning disable CS8604 // Possible null reference argument.
                    SyncMemberEditorBuilder.Build(abstractedDynVar.NameField, "Name", PropertyAccess.TryGetSyncMemberFieldInfo(abstractedDynVar.NameField), builder);
#pragma warning restore CS8604 // Possible null reference argument.
                    builder.PopStyle();
                    builder.NestOut();
                    nLines++;
                }
            }
        }
        return nLines;
    }

    public void Setup(UIBuilder builder) {
        throw new NotImplementedException();
    }
}
