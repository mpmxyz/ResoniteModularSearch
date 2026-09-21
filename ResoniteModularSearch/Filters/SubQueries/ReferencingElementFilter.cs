
using Elements.Core;

using FrooxEngine;

using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch.Filters.SubQueries;

[Filter("Referencing")]
public class ReferencingElementFilter<TElement> : SubQueryFilterBase where TElement : IWorldElement {
    public override string Name => $"Referencing {typeof(TElement).GetNiceName()}";

    public override ISearchSource GetSubQueryElements(IWorldElement element) {
        if (element is ISyncRef syncRef && syncRef.Target is TElement targetElement) {
            return new ConstantSource([targetElement], [targetElement]);
        } else {
            return ConstantSource.Empty;
        }
    }
}
