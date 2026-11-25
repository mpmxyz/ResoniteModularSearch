namespace ResoniteModularSearch.DataModel;

public class PropertyUI(Action onRefresh) {
    //TODO: convert UI-building code to use a stateful builder -> allows collecting all UI-editable values -> enable save/load as well as an API for outside tools
    private readonly Action onRefresh = onRefresh;

    public void Refresh() {
        onRefresh();
    }
}
