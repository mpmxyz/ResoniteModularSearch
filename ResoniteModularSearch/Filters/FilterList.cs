
using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.Operations;

namespace ResoniteModularSearch.Filters;

/// <summary>
/// This filter will combine filters by mapping the boolean result of Match() to the decisions Allow/Deny/Neutral.
/// If all decisions are neutral, a default is applied.
/// </summary>
internal class FilterList : IFilter {
    public readonly struct FilterConfig(IFilter filter, bool? onMatch, bool? onMismatch) {
        public IFilter Filter { get; } = filter;
        public bool? OnMatch { get; } = onMatch;
        public bool? OnMismatch { get; } = onMismatch;
    }

    public List<FilterConfig> FilterConfigs { get; set; } = [];
    public bool DefaultResult = true;

    public string Name => "Filter List";

    public bool IsValid => FilterConfigs.All((it) => it.Filter.IsValid);

    public List<IFilterAction> FilterActions => [];

    public void Setup(Slot slot, UIBuilder builder) {
        //TODO: create generic type picker for instantiable subtypes of IFilter
        //TODO: create entry + UI
        //TODO: add UI for filter options
        //      UI idea after initial prototype: tabular layout and radio buttons
        throw new NotImplementedException();
    }


    public bool Match(IWorldElement element) {
        bool? result = null;
        foreach (var config in FilterConfigs) {
            if (config.Filter.Match(element)) {
                result = config.OnMatch;
                if (result.HasValue) {
                    return result.Value;
                }
            } else {
                result = config.OnMismatch;
                if (result.HasValue) {
                    return result.Value;
                }
            }
        }
        return DefaultResult;
    }
}
