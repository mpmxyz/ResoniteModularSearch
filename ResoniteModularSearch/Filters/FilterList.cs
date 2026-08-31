using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Operations;

namespace ResoniteModularSearch.Filters;

/// <summary>
/// This filter will combine filters by mapping the boolean result of Match() to the decisions Allow/Deny/Neutral.
/// If all decisions are neutral, a default is applied.
/// </summary>
[Filter("Filter List")]
internal class FilterList : IFilter {
    public class FilterConfig(IFilter filter, bool? onMatch, bool? onMismatch) {
        public IFilter Filter { get; } = filter;
        public bool? OnMatch { get; set; } = onMatch;
        public bool? OnMismatch { get; set; } = onMismatch;
    }

    public List<FilterConfig> FilterConfigs { get; set; } = [];
    public bool DefaultResult = true;

    public event Action<FilterConfig>? OnFilterAdded;
    public event Action<FilterConfig>? OnFilterRemoved;
    public event Action<FilterConfig>? OnFilterMoved;

    public string Name => "Filter List";

    public bool IsValid => FilterConfigs.All((it) => it.Filter.IsValid);

    private Action<IFilterAction>? _applyAction = null;
    public Action<IFilterAction>? ApplyAction {
        private get => _applyAction;
        set {
            _applyAction = value;
            foreach (var config in FilterConfigs) {
                config.Filter.ApplyAction = value;
            }
        }
    }

    private static readonly List<PropertySelection.Option<bool>> requiredResponses = [
        new(true, "Accept"),
        new(false, "Discard")
    ];
    private static readonly List<PropertySelection.Option<bool?>> optionalResponses = [
        new(true, "Accept"),
        new(null, "Continue"),
        new(false, "Discard")
    ];
    public void Setup(UIBuilder builder) {
        //      UI idea after initial prototype: tabular layout and radio buttons
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING * 2).Slot.Name+=" (Filter List)";
        {
            Slot configListSlot = builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING * 2).Slot;
            var style = builder.Style.Clone();
            Dictionary<FilterConfig, Slot> knownConfigs = [];
            void BuildFilterConfig(FilterConfig config) {
                UIBuilder builder = new(configListSlot);
                StyleHelpers.CopyStyleProperties(style, builder.Style);

                builder.NestInto(configListSlot);
                {
                    var slot = builder.HorizontalLayout(StyleHelpers.DEFAULT_SPACING).Slot;
                    slot.Destroyed += (s) => RemoveFilter(config);
                    slot.Name += " (Filter Config)";
                    knownConfigs[config] = slot;

                    builder.PushStyle();
                    builder.Style.Width = StyleHelpers.DEFAULT_HEIGHT;
                    {
                        builder.VerticalLayout().Slot.Name = "Position Arrows";
                        builder.PopStyle();
                        builder.PushStyle();
                        builder.Style.Height = StyleHelpers.DEFAULT_HEIGHT;
                        ButtonAction.Create(builder, "↑", () => MoveFilterUp(config));
                        builder.PopStyle();
                        builder.PushStyle();
                        builder.Style.FlexibleHeight = 1;
                        builder.Next("Spacer");
                        builder.PopStyle();
                        builder.PushStyle();
                        builder.Style.Height = StyleHelpers.DEFAULT_HEIGHT;
                        ButtonAction.Create(builder, "↓", () => MoveFilterDown(config));
                        builder.PopStyle();
                        builder.NestOut();
                    }

                    builder.PushStyle();
                    builder.Style.FlexibleWidth = 1;
                    {
                        builder.VerticalLayout();
                        builder.PopStyle();
                        config.Filter.Setup(builder);
                        builder.NestOut();
                    }
                    builder.PushStyle();
                    builder.Style.Width = StyleHelpers.MATCH_RESPONSE_WIDTH;
                    {
                        builder.HorizontalLayout(StyleHelpers.DEFAULT_SPACING).Slot.Name += " (Match Responses)";
                        builder.PopStyle();
                        builder.PushStyle();
                        builder.Style.Height = StyleHelpers.DEFAULT_HEIGHT;
                        builder.CreateSelection("On Match", config.OnMatch, (value) => config.OnMatch = value, optionalResponses);
                        builder.CreateSelection("Else", config.OnMismatch, (value) => config.OnMismatch = value, optionalResponses);
                        builder.PopStyle();
                        builder.NestOut();
                    }
                    builder.PushStyle();
                    builder.Style.Width = StyleHelpers.DEFAULT_HEIGHT;
                    {
                        builder.VerticalLayout().Slot.Name += " (Remove)";
                        builder.PopStyle();
                        builder.PushStyle();
                        builder.Style.Height = StyleHelpers.DEFAULT_HEIGHT;
                        ButtonAction.Create(builder, "❌", () => RemoveFilter(config));
                        builder.PopStyle();
                        builder.NestOut();
                    }
                    builder.NestOut();
                }

                builder.NestOut();
            }
            void RemoveFilterUI(FilterConfig config) {
                if (knownConfigs.TryGetValue(config, out var slot)) {
                    knownConfigs.Remove(config);
                    if (!slot.IsDestroyed) {
                        slot.Destroy();
                    }
                }
            }
            void UpdateFilterOrder(FilterConfig moved) {
                long i = 0;
                foreach(var config in FilterConfigs) {
                    if (knownConfigs.TryGetValue(config, out var slot)) {
                        slot.OrderOffset = i++;
                    }
                }
            }
            FilterConfigs.ForEach(BuildFilterConfig);
            OnFilterAdded += BuildFilterConfig;
            OnFilterRemoved += RemoveFilterUI;
            OnFilterMoved += UpdateFilterOrder;
            builder.NestOut();
        }
        {
            builder.HorizontalLayout(StyleHelpers.DEFAULT_SPACING);

            builder.PushStyle();
            builder.Style.FlexibleWidth = 1;
            World world = builder.World;
            ButtonAction.Create(builder, "Add Filter...", () => {
                new TypeSelectionInstantiator<IFilter>(AddFilter).Setup(world, "Select Filter Type");
            });
            builder.PopStyle();

            builder.PushStyle();
            builder.Style.Width = StyleHelpers.MATCH_RESPONSE_WIDTH;
            builder.CreateSelection("Default", DefaultResult, (value) => DefaultResult = value, requiredResponses);
            builder.PopStyle();

            builder.Spacer(StyleHelpers.DEFAULT_HEIGHT);

            builder.NestOut();
        }

        builder.NestOut();
    }

    public void AddFilter(IFilter filter) {
        FilterConfig newConfig = new(filter, DefaultResult ? null : true, DefaultResult ? false : null);
        FilterConfigs.Add(newConfig);
        filter.ApplyAction = ApplyAction;
        OnFilterAdded?.Invoke(newConfig);
    }
    public void RemoveFilter(FilterConfig config) {
        FilterConfigs.Remove(config);
        OnFilterRemoved?.Invoke(config);
    }
    public void MoveFilterUp(FilterConfig config) {
        int index = FilterConfigs.IndexOf(config);
        if (index > 0) {
            (FilterConfigs[index], FilterConfigs[index - 1]) = (FilterConfigs[index - 1], FilterConfigs[index]);
            OnFilterMoved?.Invoke(config);
        }
    }
    public void MoveFilterDown(FilterConfig config) {
        int index = FilterConfigs.IndexOf(config);
        if (index >= 0 && index < FilterConfigs.Count - 1) {
            (FilterConfigs[index], FilterConfigs[index + 1]) = (FilterConfigs[index + 1], FilterConfigs[index]);
            OnFilterMoved?.Invoke(config);
        }
    }


    public bool Match(IWorldElement element) {
        foreach (var config in FilterConfigs) {
            if (config.Filter.Match(element)) {
                var result = config.OnMatch;
                if (result.HasValue) {
                    return result.Value;
                }
            } else {
                var result = config.OnMismatch;
                if (result.HasValue) {
                    return result.Value;
                }
            }
        }
        return DefaultResult;
    }
}
