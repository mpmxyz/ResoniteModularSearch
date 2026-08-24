
using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.Sources;
internal class FromIWorldElement(World world) : ISearchSource {
    public static ISearchSource Create(Slot slot, UIBuilder builder) {
        return new FromIWorldElement(slot.World);
    }

    public IEnumerable<IWorldElement> RootElements => [world.RootSlot];
}
