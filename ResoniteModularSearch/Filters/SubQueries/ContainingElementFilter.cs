
using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch.Filters.SubQueries;

[Filter("Containing Element")]
public class ContainingElementFilter<TElement> : SubQueryFilterBase where TElement : IWorldElement {
    public override string Name => $"Containing {typeof(TElement).GetNiceName()}";
    //slot recursion is only enabled for type Slot by default (with depth=1 -> direct children to avoid performance issues)
    public int MaxDepth { get; set; } = typeof(TElement) == typeof(Slot) ? 1 : 0;

    public override ISearchSource GetSubQueryElements(IWorldElement element) {
        var source = new FromParent<TElement>(element) {
            IncludeSearchRoot = false,
            MaxDepth = MaxDepth
        };
        return source;
    }

    public override void Setup(UIBuilder builder) {
        builder.CreateValueEditor("Max Depth (levels of slots)", () => MaxDepth, (value) => MaxDepth = value);
        base.Setup(builder);
    }
}
