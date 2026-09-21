using System.Collections.Immutable;

using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.Sources;

public class ConstantSource(IEnumerable<IWorldElement> rootElements, IEnumerable<IWorldElement> candidates) : ISearchSource {
    public static readonly ConstantSource Empty = new([], []);

    public IEnumerable<IWorldElement> RootElements { get; } = rootElements.ToImmutableList();
    public IEnumerable<IWorldElement> Candidates { get; } = candidates.ToImmutableList();

    public IEnumerable<IWorldElement> GetAllCandidates(Func<IWorldElement, bool> mask) {
        return Candidates.Where(mask);
    }

    public void Setup(UIBuilder builder) {
        //no UI
    }
}
