
using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch.Filters.SubQueries;

[Filter("Containing Element")]
public class ContainingElementFilter<TElement> : SubQueryFilterBase where TElement : IWorldElement {
    public override string Name => $"Containing {typeof(TElement).GetNiceName()}";
    public int MaxDepth { get; set; } = -1;

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
