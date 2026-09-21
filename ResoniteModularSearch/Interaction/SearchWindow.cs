using Elements.Core;

using FrooxEngine;

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
        Setup(world.LocalUserSpace.AddSlot("Search Window"));
    }

    public void Setup(Slot slot) {
        slot.PositionInFrontOfUser(float3.Backward, distance: 1.5f);
        slot.DestroyWhenUserLeaves(slot.LocalUser);
        slot.ScaleToUser(slot.LocalUser);

        var builder = RadiantUI_Panel.SetupPanel(slot, "Search & Replace", new float2(2500f, 1000f));
        slot.Tag = "Developer";
        slot.LocalScale *= 0.001f;

        builder.Style.ForceExpandHeight = false;
        builder.Style.ChildAlignment = Alignment.TopLeft;
        builder.Canvas.AcceptPhysicalTouch.Value = false;

        builder.PushStyle();
        builder.Style.ForceExpandHeight = true;
        {
            builder.HorizontalLayout();
            builder.PopStyle();
            builder.PushStyle();
            builder.Style.Width = 1500;
            {
                builder.VerticalLayout();
                builder.PopStyle();
                SearchRequestPanel.Setup(builder);
                var previousMask = SearchRequestPanel.Mask;
                SearchRequestPanel.Mask = (element) => element != slot && (previousMask == null || previousMask(element));
                builder.NestOut();
            }
            builder.PushStyle();
            builder.Style.FlexibleWidth = 1;
            builder.Style.ForceExpandHeight = true;
            {
                builder.HorizontalLayout();
                builder.PopStyle();
                SearchResultDisplay.Setup(builder);
                builder.NestOut();
            }
            builder.NestOut();
        }
    }
}
