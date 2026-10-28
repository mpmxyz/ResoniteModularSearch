
using FrooxEngine;
using FrooxEngine.UIX;

namespace ResoniteModularSearch.DataModel;
public static class HyperlinkHelper {
    public static void Hyperlink(this UIBuilder builder, Uri link, string? text = null, string? reason = null) {
        builder.PushStyle();
        builder.Style.TextAutoSizeMin = StyleHelpers.DEFAULT_MIN_SIZE;
        builder.Style.TextAutoSizeMax = StyleHelpers.DEFAULT_MIN_SIZE;
        builder.Style.ForceExpandWidth = false;
        builder.HorizontalLayout(spacing: 8f);
        builder.FitContent(horizontal: SizeFit.PreferredSize, vertical: SizeFit.Disabled);
        text ??= link.OriginalString;
        text = string.Format("<u><noparse={0}>{1}</u>", text.Length, text);
        var linkText = builder.Text(text);
        linkText.Color.Value = RadiantUI_Constants.Hero.CYAN;
        var linkButton = linkText.Slot.AttachComponent<Button>();
        linkButton.SetupBackgroundColor(linkText.Color);
        var linkAction = linkText.Slot.AttachComponent<Hyperlink>();
        linkAction.URL.Value = link;
#pragma warning disable CS8601 // Possible null reference assignment.
        linkAction.Reason.Value = reason;
#pragma warning restore CS8601 // Possible null reference assignment.
        var copyButton = builder.Button("(copy link)");
        builder.FitContent(horizontal: SizeFit.PreferredSize, vertical: SizeFit.Disabled);
        copyButton.Slot.AttachComponent<OverlappingLayout>();
        var copySource = copyButton.Slot.AttachComponent<ValueField<string>>();
        copySource.Value.Value = link.OriginalString;
        var copyAction = copyButton.Slot.AttachComponent<ButtonClipboardCopyText>();
        copyAction.Source.Target = copySource.Value;
        builder.PopStyle();
        builder.NestOut();
    }
}
