
using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.Sources;

public class FromUsers(World world) : ISearchSource {
    public IEnumerable<IWorldElement> RootElements => world.AllUsers.ToList();

    public void Setup(UIBuilder builder) {
        //TODO: visuals
    }
}
