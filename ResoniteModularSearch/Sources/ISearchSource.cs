using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.Sources;

/// <summary>
/// 
/// </summary>
public interface ISearchSource {
    /// <summary>
    /// A list of elements to start the search from
    /// TODO: generalize/move this concept to adapt to different traversal strategies
    /// </summary>
    IEnumerable<IWorldElement> RootElements { get; }

    /// <summary>
    /// recursively queries all elements reachable from this search source
    /// TODO: make search more adaptable to queries that only search for a specific type (abstraction for non-SyncObject Worker types vs. ISyncElement vs. other)
    /// </summary>
    /// <param name="mask">evaluates to false to exclude elements and their children</param>
    /// <returns>a sequence of candidate elements</returns>
    IEnumerable<IWorldElement> GetAllCandidates(Func<IWorldElement, bool> mask);

    /// <summary>
    /// instantiates and integrates UI into a search request panel
    /// </summary>
    /// <param name="builder">can be used to create the UI for options and actions</param>
    void Setup(UIBuilder builder);
}
