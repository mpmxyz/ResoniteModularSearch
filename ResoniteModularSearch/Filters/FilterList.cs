
using System.Reflection;

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

    public void Setup(UIBuilder builder) {
        //      UI idea after initial prototype: tabular layout and radio buttons
        builder.VerticalLayout().Slot.Name+="(Filter List)";
        builder.CreateValueProperty("Default Result", DefaultResult, (value) => DefaultResult = value);
        Slot configListSlot = builder.VerticalLayout().Slot;
        var style = builder.Style.Clone();
        void BuildFilterConfig(FilterConfig config) {
            UIBuilder builder = new(configListSlot);
            StyleHelpers.CopyStyleProperties(style, builder.Style);
            builder.NestInto(configListSlot);
            builder.HorizontalLayout();
            builder.VerticalLayout();
            config.Filter.Setup(builder);
            builder.NestOut();
            builder.HorizontalLayout();
            builder.CreateValueProperty("On Match", config.OnMatch, (value) => config.OnMatch = value);
            builder.CreateValueProperty("On Mismatch", config.OnMismatch, (value) => config.OnMismatch = value);
        }
        FilterConfigs.ForEach(BuildFilterConfig);
        OnFilterAdded += BuildFilterConfig;
        builder.NestOut();
        World world = builder.World;
        ButtonAction.Create(builder, "Add Filter...", () => {
            new TypeSelectionInstantiator<IFilter>(AddFilter).Setup(world, "Select Filter Type");
        });
        builder.NestOut();
    }

    public void AddFilter(IFilter filter) {
        FilterConfig newConfig = new(filter, DefaultResult ? null : true, DefaultResult ? false : null);
        FilterConfigs.Add(newConfig);
        filter.ApplyAction = ApplyAction;
        OnFilterAdded?.Invoke(newConfig);
    }
    //TODO: Remove filter
    //TODO: Reorder filter


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
