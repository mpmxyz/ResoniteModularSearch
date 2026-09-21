using Elements.Core;

using FrooxEngine;
using FrooxEngine.Undo;

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
        slot.ScaleToUser(slot.LocalUser);

        //make sure only users with builder role can interact with it
        //Note: Creation of window is implicitly permitted by builder role since it requires interaction with a DevTool.
        slot.Tag = "Developer";

        //prevent littering sessions
        slot.PersistentSelf = false;
        slot.DestroyWhenUserLeaves(slot.LocalUser);

        //make sure noone can create broken search windows
        slot.AttachComponent<DuplicateBlock>();
        slot.AttachComponent<GrabbableSaveBlock>();
        slot.AttachComponent<NoDestroyUndo>();

        var builder = RadiantUI_Panel.SetupPanel(slot, "Search & Replace", new float2(2500f, 1000f));
        slot.LocalScale *= 0.001f;

        //prevent accidental interactions when moving through window
        builder.Canvas.AcceptPhysicalTouch.Value = false;

        builder.Style.ForceExpandHeight = false;
        builder.Style.ChildAlignment = Alignment.TopLeft;

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
