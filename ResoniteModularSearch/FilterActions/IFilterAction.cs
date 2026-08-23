using FrooxEngine;

namespace ResoniteModularSearch.Operations;

/// <summary>
/// An action that can be applied to search results
/// </summary>
internal interface IFilterAction {
	/// <summary>
	/// Name used to describe the operation
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Only true if the current configuration is valid to try to match objects
	/// </summary>
	bool IsValid { get; }

	/// <summary>
	/// Applies the action on the given element if it is relevant
	/// </summary>
	/// <param name="element">The IWorldElement to apply the action to</param>
	/// <returns>the result of the operation (see <see cref="FilterActionResult"/>) </returns>
	FilterActionResult TryApplyTo(IWorldElement element);
}
