using Elements.Core;

using FrooxEngine;
using FrooxEngine.Undo;

namespace ResoniteModularSearch.DataModel;
public static class PropertyAccess {
    public record struct PropertyStatistics(int TotalCount, int UndoableCount) {
        public static readonly PropertyStatistics None = new(0, 0);
        public readonly bool FullyUndoable => TotalCount == UndoableCount;
        public static PropertyStatistics operator +(PropertyStatistics left, PropertyStatistics right) {
            return new(TotalCount: left.TotalCount + right.UndoableCount,
                       UndoableCount: left.UndoableCount + right.UndoableCount);
        }
    }

    /// <summary>
    /// Counts all values matching the predicate that are contained within the given objects.
    /// This will only check non-IWorldElement properties of the given element. (i.e. no Sync or SyncObject)
    /// </summary>
    /// <typeparam name="T">expected type of the properties within the object</typeparam>
    /// <param name="element">is checked for valuess</param>
    /// <param name="predicate">checks if a value is accepted</param>
    /// <returns>the number of matching values contained within the object and how many of those could be undone after running <see cref="UndoableEditAllMatchingValues"/></returns>
    public static PropertyStatistics CountMatchingValues<T>(IWorldElement element, Func<T, bool> predicate) {
        PropertyStatistics result = PropertyStatistics.None;
        if (element is IValue<T> value) {
            if (predicate(value.Value)) {
                if (element is IField<T> field) {
                    result.UndoableCount++;
                }
                result.TotalCount++;
            }
        }
        return result;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="element"></param>
    /// <param name="predicate"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static PropertyStatistics UndoableEditAllMatchingValues<T>(IWorldElement element, Func<T, bool> predicate, Func<T, T> transform) {
        PropertyStatistics replacementCount = PropertyStatistics.None;
        if (element is IValue<T> value) {
            T oldValue = value.Value;
            if (predicate(oldValue)) {
                T newValue = transform(oldValue);
                if (!Coder<T>.Equals(newValue, oldValue)) {
                    if (element is IField<T> field) {
                        field.CreateUndoPoint();
                        replacementCount.UndoableCount++;
                    }
                    value.Value = newValue;
                    replacementCount.TotalCount++;
                }
            }
        }
        return replacementCount;
    }
}
