
using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch.Filters.SubQueries;

[Filter("Within Element")]
public class WithinElementFilter<TElement> : SubQueryFilterBase where TElement : IWorldElement {
    public override string Name => $"Within {typeof(TElement).GetNiceName()}";
    public int MaxDepth { get; set; } = -1;

    public override ISearchSource GetSubQueryElements(IWorldElement element) {
        var source = new ToParent<TElement>(element) {
            IncludeSearchRoot = false,
            MaxDepth = MaxDepth
        };
        return source;
    }

    public override void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING); //TODO: move vertical layout out of filter UI generation -> use super setup
        builder.CreateValueEditor("Max Depth (levels of slots)", MaxDepth, (value) => MaxDepth = value);
        FilterList.Setup(builder);
        builder.NestOut();
    }
}
