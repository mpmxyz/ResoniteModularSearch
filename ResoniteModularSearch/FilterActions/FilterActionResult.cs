namespace ResoniteModularSearch.FilterActions;

public record struct FilterActionResult(int SuccessCount, int FailureCount) {
	/// <summary>
	/// An error occurred while applying the operation.
	/// </summary>
	public static readonly FilterActionResult Failed = new(0, 1);

    /// <summary>
    /// The operation was applied successfully.
    /// </summary>
    public static readonly FilterActionResult Success = new(1, 0);

    /// <summary>
    /// Nothing was done.
    /// </summary>
    public static readonly FilterActionResult Ignored = new(0, 0);

    public static FilterActionResult operator +(FilterActionResult a, FilterActionResult b) {
        return new(a.SuccessCount + b.SuccessCount, a.FailureCount + b.FailureCount);
    }
}
