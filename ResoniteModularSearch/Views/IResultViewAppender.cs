
using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Views;

/// <summary>
/// Appenders can add UI elements for a given element
/// </summary>
public interface IResultViewAppender : IResultViewModule {
    /// <summary>
    /// used to define the order between appenders (lowest are first)
    /// </summary>
    int AppendOffset { get; set; }

    /// <summary>
    /// tries to append UI for the given element, does nothing if the appender cannot handle the element
    /// </summary>
    /// <param name="builder">used to create UI</param>
    /// <param name="element">to be displayed</param>
    /// <param name="result">search result the displayed element is part of</param>
    /// <param name="composition">composition of view modules that can be used to check for masking or to create views for additional items</param>
    /// <returns>number of lines added to the UI (used to limit max. number of elements that are revealed in UI at once)</returns>
    int TryAppendUI(UIBuilder builder, IWorldElement element, SearchResult result, ResultViewModuleComposition composition);
}
