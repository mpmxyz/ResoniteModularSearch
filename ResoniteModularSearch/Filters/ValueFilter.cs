using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters;

[Filter("Value")]
public class ValueFilter<T> : IFilter {
    public class ReplaceAction : IFilterAction {
        private readonly ValueFilter<T> filter;

        public ReplaceAction(ValueFilter<T> filter) {
            this.filter = filter;
        }

        public string Name => "Replace";
        public bool IsValid => filter.IsValid;

        public FilterActionResult TryApplyTo(IWorldElement element, SearchContext context) {
            T searchFor = filter.SearchFor;
            T replaceWith = filter.ReplaceWith;
            PropertyAccess.PropertyStatistics result = PropertyAccess.UndoableEditAllMatchingValues<T>(
                                                element: element,
                                                //This does not match using the regex as non-matching values will be ignored anyway.
                                                //(Replace causes no change -> ignored)
                                                predicate: (value) => Object.Equals(value, searchFor),
                                                transform: (value) => replaceWith);
            if (result.TotalCount == 0) {
                return FilterActionResult.Ignored;
            } else {
                return FilterActionResult.Success;
            }
        }
    }

    public static bool IsValidType => Coder<T>.IsEnginePrimitive;

    public T SearchFor { get; set; } = Coder<T>.Default;
    public T ReplaceWith { get; set; } = Coder<T>.Default;

    public string Name => $"Value<{typeof(T).GetNiceName()}>";

    public bool IsValid { get => IsValidType; }

    public Action<IFilterAction>? ApplyAction { private get; set; }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        builder.CreateValueEditor("Search for", SearchFor, (value) => SearchFor = value);
        builder.CreateValueEditor("Replace with", ReplaceWith, (value) => ReplaceWith = value);
        ButtonAction.Create(builder, "Replace", () => ApplyAction?.Invoke(new ReplaceAction(this)));
        builder.NestOut();
    }

    public bool Match(IWorldElement element, SearchContext context) {
        T searchFor = SearchFor;
        PropertyAccess.PropertyStatistics result = PropertyAccess.CountMatchingValues<T>(
            element: element,
            predicate: (value) => Equals(value, searchFor));
        return result.TotalCount != 0;
    }
}
