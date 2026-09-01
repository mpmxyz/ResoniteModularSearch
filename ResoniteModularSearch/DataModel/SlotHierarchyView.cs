using Elements.Core;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.DataModel;
public class SlotHierarchyView(Slot slot, SearchResult result) {

    public Slot Slot { get; } = slot;
    private SearchResult Result { get; } = result;

    private bool _opened = false;
    public bool Opened { 
        get => _opened;
        set {
            if (value != _opened) {
                _opened = value;
                OnOpenChanged?.Invoke(value);
            }
        }
    }

    private event Action<bool>? OnOpenChanged;

    private readonly List<PropertySelection.Option<bool>> visibilityOptions =
    [
        new(false, "▶"),
        new(true, "▼")
    ];
    private readonly List<PropertySelection.Option<bool>> noChildOptions =
    [
        new(false, "●")
    ];

    public void TryBuild(UIBuilder builder, SyncRef<IWorldElement> filterProperty) {
        if (!Result.RecursiveResultCount.ContainsKey(Slot)) {
            return;
        }

        UIStyle style = builder.Style.Clone();

        var rootSlot = builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING).Slot;
        Slot.Destroyed += (x) => rootSlot.Destroy();
        {
            builder.PushStyle();
            builder.Style.Height = StyleHelpers.DEFAULT_MIN_SIZE;
            builder.HorizontalLayout(StyleHelpers.DEFAULT_SPACING);
            builder.Style.Width = StyleHelpers.DEFAULT_MIN_SIZE;
            if (Result.RecursiveResultCount.GetValueOrDefault(Slot, 0) > Result.DirectSlotResultCount.GetValueOrDefault(Slot, 0)) {
                //There are children: make visibility depend on 
                builder.CreateSelection(null, Opened, (value) => Opened = value, visibilityOptions);
            } else {
                builder.CreateSelection(null, false, (value) => { }, noChildOptions);
            }
            builder.PopStyle();
            builder.PushStyle();
            builder.Style.FlexibleWidth = 1;
            builder.Style.MinWidth = StyleHelpers.MINIMUM_SLOT_NAME_WIDTH;
            Text nameText = builder.Text(null, bestFit: true, Alignment.MiddleLeft);
            builder.PopStyle();
            void UpdateNameText() {
                var recursiveCount = Result.RecursiveResultCount.GetValueOrDefault(Slot, 0);
                var directCount = Result.DirectSlotResultCount.GetValueOrDefault(Slot, 0);
                nameText.Content.Value = $"({directCount}/{recursiveCount}) {Slot.Name}";
            }
            UpdateNameText();
            Slot.Name_Field.Changed += (x) => UpdateNameText();

            Button nameButton = nameText.Slot.AttachComponent<Button>();
            nameButton.RequireLockInToPress.Value = true;
            InteractionElement.ColorDriver colorDriver = nameButton.ColorDrivers.Add();
            colorDriver.ColorDrive.Target = nameText.Color;
            RadiantUI_Constants.SetupLabelDriverColors(colorDriver);
            nameButton.Slot.AttachComponent<ReferenceProxySource>().Reference.Target = Slot;
            var referenceSet = nameButton.Slot.AttachComponent<ButtonReferenceSet<IWorldElement>>();
            referenceSet.SetReference.Target = Slot;
            referenceSet.TargetReference.Target = filterProperty;

            builder.NestOut();
        }
        {
            builder.PushStyle();
            var childContainerLayout = builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
            childContainerLayout.PaddingLeft.Value = StyleHelpers.DEFAULT_MIN_SIZE + StyleHelpers.DEFAULT_SPACING;
            var childContainer = childContainerLayout.Slot;
            builder.PopStyle();

            childContainer.ActiveSelf = Opened;
            void CreateChildrenWhenFirstOpened(bool isOpen) {
                if (isOpen) {
                    OnOpenChanged -= CreateChildrenWhenFirstOpened;
                    UIBuilder childBuilder = new(childContainer);
                    StyleHelpers.CopyStyleProperties(style, childBuilder.Style);

                    childBuilder.NestInto(childContainer);
                    childBuilder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
                    foreach (var child in Slot.Children) {
                        new SlotHierarchyView(child, Result).TryBuild(childBuilder, filterProperty);
                    }
                    childBuilder.NestOut();
                }
            }
            builder.NestOut();
            OnOpenChanged += (opened) => childContainer.ActiveSelf = opened;
            OnOpenChanged += CreateChildrenWhenFirstOpened;
            CreateChildrenWhenFirstOpened(Opened);
        }
        builder.NestOut();

    }

}
