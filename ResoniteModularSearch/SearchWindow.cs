using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch;
internal class SearchWindow {
    public static SearchWindow Create(Slot slot) {
        var builder = new UIBuilder(slot);
        builder = RadiantUI_Panel.SetupPanel(slot, "", new float2(2f, 1f));
        builder.Style.ForceExpandHeight = false;
        builder.Style.ChildAlignment = Alignment.TopLeft;
        builder.Canvas.UnitScale.Value = 1000;
        var columns = builder.SplitHorizontally([3,2]);
        builder.NestInto(columns[0]);
        var searchRequestPanel = SearchRequestPanel.Create(slot, builder, (element) => element != slot);
        builder.NestOut();
        builder.NestInto(columns[1]);
        var searchDisplay = SearchResultDisplay.Create(slot, builder);
        builder.NestOut();
        searchRequestPanel.SearchCompleted += (result) => searchDisplay.DisplayedResult = result;
        return new SearchWindow();
    }
}
