
using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Operations;

namespace ResoniteModularSearch.Filters;
internal class ReferenceFilter : IFilter {
    internal IWorldElement? SearchFor { get; set; }
    internal IWorldElement? ReplaceWith { get; set; }
    public bool IsInverted { get; set; }

    public string Name => "Reference";

    public bool IsValid => SearchFor == null || !SearchFor.IsRemoved;

    public List<IFilterAction> FilterActions => [new ReplaceAction(this)];

    public void Setup(Slot slot, UIBuilder builder) {
        slot.CreateReferenceProperty(builder, "Search for", SearchFor, (value) => SearchFor = value);
        slot.CreateReferenceProperty(builder, "Replace with", ReplaceWith, (value) => ReplaceWith = value);
        slot.CreateValueProperty(builder, "Invert Match", IsInverted, (value) => IsInverted = value);
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
                    try{
#pragma warning disable CS8601 // Possible null reference assignment.
                        //TODO: undo point
                        syncRef.Target = filter.ReplaceWith;
#pragma warning restore CS8601 // Possible null reference assignment.
                    } catch (Exception e) {
                        if (ResoniteModularSearch.LogExceptions) {
                            ResoniteModularSearch.Error($"Failed to write replacement value {filter.ReplaceWith} to: {element}\n{e}");
                        }
                        return FilterActionResult.Failed;
                    }
                    return FilterActionResult.Success;
                }
            }
            return FilterActionResult.Ignored;
        }
    }
}
