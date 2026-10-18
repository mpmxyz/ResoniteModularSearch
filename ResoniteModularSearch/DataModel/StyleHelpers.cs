using System.Reflection;

using Elements.Core;

using FrooxEngine.UIX;

namespace ResoniteModularSearch.DataModel;
public static class StyleHelpers {
    public const float DEFAULT_MIN_SIZE = 24f;
    public const float MATCH_RESPONSE_WIDTH = 300f;
    public const float MINIMUM_SLOT_NAME_WIDTH = 200f;
    public const float DEFAULT_SPACING = 4f;
    public const float MINIMUM_FILTER_LIST_WIDTH = 700f;
    public const float WORKER_HEADER_HEIGHT = 32;
    public const float FILTER_SELECTOR_ITEM_HEIGHT = 48f;
    public static readonly float2 FILTER_SELECTOR_CANVAS_SIZE = new(400f,800f);

    public static void CopyStyleProperties(UIStyle source, UIStyle target) {
        CopyProperties(source, target);
    }

    private static void CopyProperties<T>(T source, T target) {
        FieldInfo[] fields = typeof(T).GetFields();
        foreach (var field in fields) {
            try {
                if (!field.IsInitOnly && field.IsPublic && !field.IsStatic) {
                    field.SetValue(target, field.GetValue(source));
                }
            } catch (Exception e) {
                ResoniteModularSearch.Warn($"{field.Name}: {e}");
            }
        }
    }
}
