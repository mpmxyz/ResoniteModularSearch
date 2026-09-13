using System.Reflection;

using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.Filters;

namespace ResoniteModularSearch.DataModel;
public class FilterSelectionInstantiator<T> {
    public event Action<T> OnFilterSelection;
    public Func<Type, bool>? TypeFilter;

    private readonly struct KnownFilter(Type type, string name) {
        public Type Type { get; } = type;
        public string Name { get; } = name;
    }

    private static readonly IList<KnownFilter> KnownFilters = DetectTypes();
    private static List<KnownFilter> DetectTypes() {
        List<KnownFilter> results = [];
        ResoniteModularSearch.Msg($"Search filters that are a subtype of {typeof(T).FullName}");
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            try {
                foreach (var type in assembly.GetTypes()) {
                    if (type.IsAssignableTo(typeof(T))) {
                        foreach (var cons in type.GetConstructors()) {
                            if (cons.GetParameters().Length == 0) {
                                var attribute = type.GetCustomAttribute<FilterAttribute>();
                                results.Add(new(type, attribute?.Name ?? type.Name));
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

    public FilterSelectionInstantiator(Action<T> onTypeSelection, Func<Type, bool>? typeFilter = null) {
        OnFilterSelection = onTypeSelection;
        TypeFilter = typeFilter;
    }

    private static bool IsValid(Type? type) {
        if (type == null) {
            return false;
        }
        if (type.ContainsGenericParameters) {
            return false;
        }
        PropertyInfo? property = type.GetProperty(nameof(IFilter.IsValidType), BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        if (property == null) {
            return true;
        }
        if (property.GetValue(null) is bool result) {
            return result;
        }
        return false;
    }

    private void TrySelect(Type? type) {
        if (type == null) {
            return;
        }
        if (!IsValid(type)) {
            return;
        }
        var cons = type.GetConstructor([]);
        if (cons == null) {
            return;
        }
        T? filter = (T)cons.Invoke([]);
        if (filter != null) {
            OnFilterSelection(filter);
        }
    }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
        foreach (var knownType in KnownFilters) {
            builder.Style.MinHeight = 0; //???
            builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
            if (TypeFilter == null || TypeFilter(knownType.Type)) {
                Type baseType = knownType.Type;
                Type? offeredType = null;
                var button = ButtonAction.Create(builder, knownType.Name, () => TrySelect(offeredType));
                void UpdateOfferedType(Type? type) {
                    offeredType = type;
                    button.Enabled = IsValid(type);
                }
                UpdateOfferedType(baseType);

                var typeArgs = new Type?[baseType.GetGenericArguments().Length];
                void UpdateTypeArgument(int index, Type? type) {
                    typeArgs[index] = type;
                    var appliedArgs = new Type[typeArgs.Length];
                    for (int i = 0; i < typeArgs.Length; i++) {
                        Type? arg = typeArgs[i];
                        if (arg == null) {
                            UpdateOfferedType(null);
                            return;
                        } else {
                            appliedArgs[i] = arg;
                        }
                    }
                    try {
                        UpdateOfferedType(baseType.MakeGenericType(appliedArgs));
                    } catch (Exception e) {
                        if (ResoniteModularSearch.LogExceptions) {
                            var typeArgString = string.Join(", ", typeArgs.Select((arg) => arg != null ? arg.ToString() : "null"));
                            ResoniteModularSearch.Msg($"Invalid type arguments for {baseType}: {typeArgString}\n{e}");
                        }
                    }
                }
                int i = 0;
                foreach (var parameter in baseType.GetGenericArguments()) {
                    int currentIndex = i;
                    PropertyEditor.CreateTypeEditor(builder, parameter.Name, null, (type) => UpdateTypeArgument(currentIndex, type));
                    i++;
                }
            }
            builder.NestOut();
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
        OnFilterSelection += (value) => slot.Destroy();
    }
}
