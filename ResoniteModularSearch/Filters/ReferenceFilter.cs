
using FrooxEngine;
using FrooxEngine.UIX;
using FrooxEngine.Undo;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters;

[Filter("Reference")]
public class ReferenceFilter : IFilter {
    public class ReplaceAction : IFilterAction {
        private readonly ReferenceFilter filter;

        public ReplaceAction(ReferenceFilter filter) {
            this.filter = filter;
        }

        public string Name => "Replace";
        public bool IsValid => filter.IsValid && (filter.ReplaceWith == null || !filter.ReplaceWith.IsRemoved);

        public FilterActionResult TryApplyTo(IWorldElement element, SearchContext context) {
            if (filter.Match(element, context)) {
                if (element is ISyncRef syncRef) {
                    syncRef.CreateUndoPoint();
#pragma warning disable CS8601 // Possible null reference assignment.
                    syncRef.Target = filter.ReplaceWith;
#pragma warning restore CS8601 // Possible null reference assignment.
                    return FilterActionResult.Success;
                }
            }
            return FilterActionResult.Ignored;
        }
    }
    public IWorldElement? SearchFor { get; set; }
    public IWorldElement? ReplaceWith { get; set; }
    public bool IsInverted { get; set; }

    public string Name => "Reference";

    public bool IsValid => SearchFor == null || !SearchFor.IsRemoved;

    public Action<IFilterAction>? ApplyAction { private get; set; }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        builder.CreateReferenceEditor("Search for", SearchFor, (value) => SearchFor = value);
        builder.CreateValueEditor("Invert Match", IsInverted, (value) => IsInverted = value);
        builder.CreateReferenceEditor("Replace with", ReplaceWith, (value) => ReplaceWith = value);
        ButtonAction.Create(builder, "Replace", () => ApplyAction?.Invoke(new ReplaceAction(this)));
        builder.NestOut();
    }

    public bool Match(IWorldElement element, SearchContext context) {
        if (element is ISyncRef syncRef) {
            bool doesMatch = syncRef.Target == SearchFor;
            return doesMatch != IsInverted;
        }
        return false;
    }
}
