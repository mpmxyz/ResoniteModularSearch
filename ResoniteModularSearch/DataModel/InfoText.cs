using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.DataModel;
public class InfoText(Text text) {
    private readonly Text text = text;

    public string Text { get => text.Content.Value; set => text.Content.Value = value; }

    public static InfoText Create(Slot data, UIBuilder builder, string initial) {
        var text = builder.Text(initial);
        return new InfoText(text);
    }
}
