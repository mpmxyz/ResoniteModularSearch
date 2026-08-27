using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Filters;
using ResoniteModularSearch.Search;
using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch;
internal class SearchWindow {
    public SearchResult LastResult { get; private set; } = new SearchResult(new HashSet<IWorldElement>());

    public static SearchWindow Create(Slot slot, UIBuilder builder) {
        builder=RadiantUI_Panel.SetupPanel(slot, "", new float2(500, 500));
        slot.PositionInFrontOfUser(float3.Backward);
        slot.DestroyWhenUserLeaves(slot.LocalUser);
        builder.VerticalLayout();

        SearchWindow window = new();
        ISearchSource dummySource = FromIWorldElement.Create(slot, builder);
        IFilter dummyFilter = RegexFilter.Create(slot, builder);
        InfoText? info = null;
        foreach (var action in dummyFilter.FilterActions) {
            ButtonAction.Create(slot, builder, action.Name, () => {
//TODO - bug: search config replaces itself
                int nSuccess = 0;
                int nFailed = 0;
                foreach(var item in window.LastResult.Results) {
                    switch(action.TryApplyTo(item)) {
                        case Operations.FilterActionResult.Success:
                            nSuccess++;
                            break;
                        case Operations.FilterActionResult.Failed:
                            nFailed++;
                            break;
                    }
                }
                ResoniteModularSearch.Msg($"{nSuccess} replacements, {nFailed} failures");
                if (info != null) {
                    info.Text = $"{nSuccess} replacements, {nFailed} failures";
                }
            });
        }
        ButtonAction.Create(slot, builder, "Search", () => {
            var results = SearchAlgorithm.RunSearch(dummySource, dummyFilter);
            window.LastResult = results;
            ResoniteModularSearch.Msg($"Found {results.Results.Count} match(es)");
            if (info != null) {
                info.Text = $"Found {results.Results.Count} match(es)";
            }
        });
        info = InfoText.Create(slot, builder, "");
        return window;
    }
}
