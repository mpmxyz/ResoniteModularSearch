
using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.Sources;
internal class FromUsers(World world) : ISearchSource {
    public static ISearchSource Create(Slot slot, UIBuilder builder) {
        return new FromUsers(slot.World);
    }

    public IEnumerable<IWorldElement> RootElements => world.AllUsers.ToList();

}
