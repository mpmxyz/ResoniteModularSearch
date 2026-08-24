using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.Sources;
/// <summary>
/// 
/// </summary>
internal interface ISearchSource {
    /// <summary>
    /// Instantiates a search source and integrates its UI into a search window
    /// </summary>
    /// <param name="slot">can be used to store data</param>
    /// <param name="builder">can be used to create the UI for options and actions</param>
    /// <returns>a new filter instance</returns>
    static abstract ISearchSource Create(Slot slot, UIBuilder builder);
    
    /// <summary>
    /// A list of elements to start the search from
    /// </summary>
    IEnumerable<IWorldElement> RootElements { get; }
}
