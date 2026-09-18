

using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.Sources;

public class FromUsers(World world) : ISearchSource {
    public IEnumerable<IWorldElement> RootElements => world.AllUsers.ToList();

    public IEnumerable<IWorldElement> GetAllCandidates(Func<IWorldElement, bool> mask) {
        throw new NotImplementedException();
    }

    public void Setup(UIBuilder builder) {
        //TODO: visuals
    }
}
