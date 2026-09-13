using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.Sources;
/// <summary>
/// 
/// </summary>
public interface ISearchSource {
    /// <summary>
    /// A list of elements to start the search from
    /// </summary>
    IEnumerable<IWorldElement> RootElements { get; }

    /// <summary>
    /// instantiates and integrates UI into a search request panel
    /// </summary>
    /// <param name="builder">can be used to create the UI for options and actions</param>
    void Setup(UIBuilder builder);
}
