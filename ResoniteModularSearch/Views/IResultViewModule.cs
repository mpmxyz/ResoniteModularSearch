using FrooxEngine.UIX;

namespace ResoniteModularSearch.Views;
public interface IResultViewModule {
    /// <summary>
    /// instantiates and integrates UI into a search request panel
    /// </summary>
    /// <param name="builder">can be used to create the UI for options and actions</param>
    void Setup(UIBuilder builder);
}
