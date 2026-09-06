using System.Reflection;

using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.Filters;

namespace ResoniteModularSearch.DataModel;
public class TypeSelectionInstantiator<T> {
    public event Action<T> OnTypeSelection;
    public Func<Type, bool>? TypeFilter;

    private readonly struct KnownConstructor(Type type, string name, Func<T> constructor) {
        public Type Type { get; } = type;
        public string Name { get; } = name;
        public Func<T> Constructor { get; } = constructor;
    }

    private static readonly IList<KnownConstructor> KnownConstructors = ConstructConstructors();
    private static List<KnownConstructor> ConstructConstructors() {
        List<KnownConstructor> results = [];
        ResoniteModularSearch.Msg($"Search {typeof(T).FullName}");
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            try {
                foreach (var type in assembly.GetTypes()) {
                    if (type.IsAssignableTo(typeof(T))) {
                        foreach (var cons in type.GetConstructors()) {
                            if (cons.GetParameters().Length == 0) {
                                var attribute = type.GetCustomAttribute<FilterAttribute>();
                                results.Add(new(type, attribute?.Name ?? type.Name, () => (T)cons.Invoke([])));
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                ResoniteModularSearch.Warn(ex.ToString());
            }
        }
        return results.OrderBy((it) => it.Name).ToList();
    }

    public TypeSelectionInstantiator(Action<T> onTypeSelection, Func<Type, bool>? typeFilter = null) {
        OnTypeSelection = onTypeSelection;
        TypeFilter = typeFilter;
    }


    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        foreach (var knownConstructor in KnownConstructors) {
            ResoniteModularSearch.Msg(knownConstructor.Name);
            if (TypeFilter == null || TypeFilter(knownConstructor.Type)) {
                ButtonAction.Create(builder, knownConstructor.Name, () => OnTypeSelection(knownConstructor.Constructor()));
            }
        }
        builder.NestOut();
    }

    public void Setup(World world, string title, User? user = null) {
        Slot slot = world.LocalUserSpace.AddSlot(title);
#pragma warning disable CS8604 // Possible null reference argument.
        //TODO: create homegrown solution because the mod owner's viewing angle is applied even if user!=null
        slot.PositionInFrontOfUser(float3.Backward, user: user);
#pragma warning restore CS8604 // Possible null reference argument.
        slot.DestroyWhenUserLeaves(slot.LocalUser);
        slot.ScaleToUser(slot.LocalUser);
        var builder = RadiantUI_Panel.SetupPanel(slot, $"{title}...", new float2(0.4f,0.8f), pinButton: false);
        builder.Style.MinHeight = 48f;
        builder.Style.ForceExpandHeight = false;
        builder.Canvas.UnitScale.Value = 1000f;
        builder.ScrollArea();
        builder.FitContent(SizeFit.Disabled, SizeFit.MinSize);
        Setup(builder);
        OnTypeSelection += (value) => slot.Destroy();
    }
}
