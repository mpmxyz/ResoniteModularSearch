using FrooxEngine.UIX;

namespace ResoniteModularSearch.DataModel;
public class InfoText(Text text) {
    private readonly Text text = text;

    public string Text { get => text.Content.Value; set => text.Content.Value = value; }

    public static InfoText Create(UIBuilder builder, string initial) {
        builder.PushStyle();
        builder.Style.Height = StyleHelpers.DEFAULT_HEIGHT * 3;
        var text = builder.Text(initial);
        builder.PopStyle();
        return new InfoText(text);
    }
}
