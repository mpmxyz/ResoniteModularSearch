namespace ResoniteModularSearch.DataModel;

public class PropertyUI(Action onRefresh) {
    private readonly Action onRefresh = onRefresh;

    public void Refresh() {
        onRefresh();
    }
}
