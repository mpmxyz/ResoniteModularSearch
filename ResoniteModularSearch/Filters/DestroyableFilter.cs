using FrooxEngine;
using FrooxEngine.UIX;
using FrooxEngine.Undo;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.FilterActions;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Filters;

[Filter("Destroyable")]
public class DestroyableFilter : IFilter {
    public class DestroyAction : IFilterAction {
        private readonly DestroyableFilter filter;

        public DestroyAction(DestroyableFilter filter) {
            this.filter = filter;
        }

        public string Name => "Destroy";
        public bool IsValid => filter.IsValid;

        public FilterActionResult TryApplyTo(IWorldElement element, SearchContext context) {
            if (element is not IDestroyable destroyable || destroyable.IsDestroyed) {
                return FilterActionResult.Ignored;
            }
            if (destroyable is Slot slot) {
                slot.UndoableDestroy();
            } else if (destroyable is Component component) {
                component.UndoableDestroy();
            } else {
                //cannot be undone
                destroyable.Destroy();
            }
            return FilterActionResult.Success;
        }
    }

    public string Name => "Destroyable";

    public bool IsValid { get => true; }

    public Action<IFilterAction>? ApplyAction { private get; set; }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        ButtonAction.Create(builder, "Destroy all search results", () => ApplyAction?.Invoke(new DestroyAction(this)), dangerous: true);
        builder.NestOut();
    }

    public bool Match(IWorldElement element, SearchContext context) {
        return element is IDestroyable;
    }
}
