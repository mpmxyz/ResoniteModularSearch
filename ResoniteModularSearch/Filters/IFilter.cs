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
	/// Only true if the current configuration is valid to try to match objects
	/// </summary>
	bool IsValid { get; }

	/// <summary>
	/// This property allows users to temporarily disable a filter.
	/// </summary>
	bool IsActive { get; set; }

	/// <summary>
	/// A list of actions supported by this filter (i.e. "Replace")
	/// </summary>
	List<IFilterAction> FilterAction { get; }

	/// <summary>
	/// Tries to match an element with the given filter.
	/// </summary>
	/// <param name="element">Slot, component, field or similar that should be checked.</param>
	/// <returns>true, if the element matches the criteria given by the filter</returns>
	bool Matches(IWorldElement element);

	/// <summary>
	/// Creates the UI used to configure the filter
	/// </summary>
	/// <param name="builder"></param>
	void BuildUI(UIBuilder builder);
}
