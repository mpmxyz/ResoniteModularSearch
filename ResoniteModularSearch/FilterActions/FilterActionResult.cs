namespace ResoniteModularSearch.FilterActions;

/// <summary>
/// Result of an <see cref="IFilterAction"/>.
/// </summary>
public enum FilterActionResult {
	/// <summary>
	/// An error occurred while applying the operation.
	/// </summary>
	Failed,
	/// <summary>
	/// The operation was applied successfully.
	/// </summary>
	Success,
	/// <summary>
	/// Nothing was done.
	/// </summary>
	Ignored
}
