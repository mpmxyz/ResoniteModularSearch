using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.Interaction;
internal class SearchWindow {
    public SearchRequestPanel SearchRequestPanel { get; }
    public SearchResultDisplay SearchResultDisplay { get; }

    public SearchWindow(World world) : this(new SearchRequestPanel(world), new SearchResultDisplay()) {

    }

    public SearchWindow(SearchRequestPanel searchRequestPanel, SearchResultDisplay searchResultDisplay) {
        SearchRequestPanel = searchRequestPanel;
        SearchResultDisplay = searchResultDisplay;
        SearchRequestPanel.SearchCompleted += (result) => SearchResultDisplay.DisplayedResult = result;
    }

    public void Setup(World world) {
        Slot slot = world.RootSlot.AddSlot("Search Window");
        slot.PositionInFrontOfUser(float3.Backward, distance: 1.5f);
        slot.DestroyWhenUserLeaves(slot.LocalUser);
        slot.ScaleToUser(slot.LocalUser);
        Setup(slot);
    }

    public void Setup(Slot slot) {
        var builder = RadiantUI_Panel.SetupPanel(slot, "Search & Replace", new float2(2f, 1f));
        slot.Tag = "Developer";


        builder.Style.ForceExpandHeight = false;
        builder.Style.ChildAlignment = Alignment.TopLeft;
        builder.Canvas.UnitScale.Value = 1000;
        builder.Canvas.AcceptPhysicalTouch.Value = false;

        var columns = builder.SplitHorizontally([2,2]);
        builder.NestInto(columns[0]);
        SearchRequestPanel.Setup(builder);
        var previousMask = SearchRequestPanel.Mask;
        SearchRequestPanel.Mask = (element) => element != slot && (previousMask == null || previousMask(element));

        builder.NestOut();
        builder.NestInto(columns[1]);
        SearchResultDisplay.Setup(builder);
        builder.NestOut();
    }
}
