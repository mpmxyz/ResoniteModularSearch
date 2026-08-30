using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.Operations;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters;

/// <summary>
/// A filter can be added to a search query to narrow down the results
/// </summary>
public interface IFilter {
	/// <summary>
	/// instantiates and integrates UI into a search request panel
	/// </summary>
	/// <param name="builder">can be used to create the UI for options and actions</param>
	void Setup(UIBuilder builder);

	/// <summary>
	/// Name used to describe the filter
	/// </summary>
	string Name { get; }

	/// <summary>
	/// True, if the filter configuration is in a valid state
	/// </summary>
	bool IsValid { get; }

	/// <summary>
	/// a callback to an element containing this filter that will run the given filter action
	/// </summary>
	Action<IFilterAction>? ApplyAction { set; }

	/// <summary>
	/// Tries to match an element with the given filter.
	/// TODO: This should be upgraded to a function returning the actual matching function. (-> create readonly copy of config)
	/// </summary>
	/// <param name="element">Slot, component, field or similar that should be checked.</param>
	/// <returns>true, if the element matches the criteria given by the filter</returns>
	bool Match(IWorldElement element);
}
