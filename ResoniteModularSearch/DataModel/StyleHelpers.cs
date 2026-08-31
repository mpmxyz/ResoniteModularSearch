using System.Reflection;

using FrooxEngine.UIX;

namespace ResoniteModularSearch.DataModel;
public static class StyleHelpers {
    public const float DEFAULT_HEIGHT = 24f;
    public const float MATCH_RESPONSE_WIDTH = 400f;
    public const float DEFAULT_SPACING = 4f;

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
