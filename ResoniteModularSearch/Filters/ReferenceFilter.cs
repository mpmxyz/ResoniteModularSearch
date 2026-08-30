
using FrooxEngine;
using FrooxEngine.UIX;
using FrooxEngine.Undo;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Operations;

namespace ResoniteModularSearch.Filters;

[Filter("Reference")]
internal class ReferenceFilter : IFilter {
    internal IWorldElement? SearchFor { get; set; }
    internal IWorldElement? ReplaceWith { get; set; }
    public bool IsInverted { get; set; }

    public string Name => "Reference";

    public bool IsValid => SearchFor == null || !SearchFor.IsRemoved;

    public Action<IFilterAction>? ApplyAction { private get; set; }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout();
        builder.CreateReferenceProperty("Search for", SearchFor, (value) => SearchFor = value);
        builder.CreateValueProperty("Invert Match", IsInverted, (value) => IsInverted = value);
        builder.CreateReferenceProperty("Replace with", ReplaceWith, (value) => ReplaceWith = value);
        ButtonAction.Create(builder, "Replace", () => ApplyAction?.Invoke(new ReplaceAction(this)));
        builder.NestOut();
    }

    public bool Match(IWorldElement element) {
        if (element is ISyncRef syncRef) {
            bool doesMatch = syncRef.Target == SearchFor;
            return doesMatch != IsInverted;
        }
        return false;
    }

    private class ReplaceAction : IFilterAction {
        private readonly ReferenceFilter filter;

        public ReplaceAction(ReferenceFilter filter) {
            this.filter = filter;
        }

        public string Name => "Replace";
        public bool IsValid => filter.IsValid && (filter.ReplaceWith == null || !filter.ReplaceWith.IsRemoved);

        public FilterActionResult TryApplyTo(IWorldElement element) {
            if (filter.Match(element)) {
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
}
