using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.Operations;

namespace ResoniteModularSearch.Filters;

/// <summary>
/// A filter can be added to a search query to narrow down the results
/// </summary>
internal interface IFilter {
	/// <summary>
	/// Instantiates a filter and integrates its UI into a search window
	/// </summary>
	/// <param name="slot">can be used to store data</param>
	/// <param name="builder">can be used to create the UI for options and actions</param>
	/// <returns>a new filter instance</returns>
	static abstract IFilter Create(Slot slot, UIBuilder builder);

	/// <summary>
	/// Name used to describe the filter
	/// </summary>
	string Name { get; }

	/// <summary>
	/// True, if the filter configuration is in a valid state
	/// </summary>
	public bool IsValid { get; }

	/// <summary>
	/// A list of actions supported by this filter (i.e. "Replace")
	/// </summary>
	List<IFilterAction> FilterActions { get; }

	/// <summary>
	/// Tries to match an element with the given filter.
	/// TODO: This should be upgraded to a function returning the actual matching function. (-> create readonly copy of config)
	/// </summary>
	/// <param name="element">Slot, component, field or similar that should be checked.</param>
	/// <returns>true, if the element matches the criteria given by the filter</returns>
	bool Match(IWorldElement element);
}
