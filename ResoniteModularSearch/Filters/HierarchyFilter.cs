
using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Operations;

namespace ResoniteModularSearch.Filters;

[Filter("Hierarchy")]
internal class HierarchyFilter : IFilter {
    internal IWorldElement? IsChildOf { get; set; } = null;
    internal IWorldElement? IsParentOf { get; set; } = null;
    internal bool IncludeSelf { get; set; } = false;

    public string Name => "Hierarchy";

    public bool IsValid {
        get {
            if (IsParentOf != null && IsParentOf.IsRemoved) {
                return false;
            }
            if (IsChildOf != null && IsChildOf.IsRemoved) {
                return false;
            }
            return true;
        }
    }

    public Action<IFilterAction>? ApplyAction { private get; set; }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        builder.CreateReferenceEditor("Child of", IsChildOf, (value) => IsChildOf = value);
        builder.CreateReferenceEditor("Parent of", IsParentOf, (value) => IsParentOf = value);
        builder.CreateValueEditor("Include self", IncludeSelf, (value) => IncludeSelf = value);
        builder.NestOut();
    }

    public bool Match(IWorldElement element) {
        if (IsChildOf != null) {
            if (element == IsChildOf) {
                if (!IncludeSelf) {
                    return false;
                }
            } else if (!element.IsChildOfElement(IsChildOf)) {
                return false;
            }
        }
        if (IsParentOf != null) {
            if (element == IsParentOf) {
                if (!IncludeSelf) {
                    return false;
                }
            } else if (!IsParentOf.IsChildOfElement(element)) {
                return false;
            }
        }
        return true;
    }
}
