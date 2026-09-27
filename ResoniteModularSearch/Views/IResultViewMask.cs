
using FrooxEngine;

using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Views;

/// <summary>
/// Masks allow removing elements from (default) views. (example: remove distracting properties from dynamic variables)
/// </summary>
public interface IResultViewMask : IResultViewModule {
    /// <summary>
    /// used to define the order between masks (lowest are first)
    /// </summary>
    int MaskOffset { get; set; }

    /// <summary>
    /// checks if this mask applies to the checked element
    /// </summary>
    /// <param name="element">to be checked</param>
    /// <param name="result">search result the check is taking place in</param>
    /// <param name="partOf">the element this element is checked as being part of</param>
    /// <returns>true, if other modules are supposed to hide this element</returns>
    bool MasksElement(IWorldElement element, SearchResult result, IWorker? partOf);
}
