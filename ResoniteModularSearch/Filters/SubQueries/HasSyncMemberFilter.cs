
using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch.Filters.SubQueries;

[Filter("Has Sync Member")]
public class HasSyncMemberFilter<TElement> : SubQueryFilterBase where TElement : ISyncMember {
    public override string Name => $"Has {typeof(TElement).GetNiceName()}";

    public override ISearchSource GetSubQueryElements(IWorldElement element) {
        var source = new FromParent<TElement>(element) {
            IncludeSearchRoot = false,
            VisitComponents = false,
            MaxDepth = 0 //no child slots
        };
        return source;
    }
}
