




using FrooxEngine;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Filters;
using ResoniteModularSearch.Filters.DynamicImpulses;
using ResoniteModularSearch.Filters.DynamicVariables;
using ResoniteModularSearch.Sources;

namespace ResoniteModularSearch.Interaction;
public static class SearchTemplate {
    public static void SearchDynamicVariableSpace(World world, DynamicVariableSpace space) {
        var window = new SearchWindow(world);
        var searchRequestPanel = window.SearchRequestPanel;
        searchRequestPanel.Source = new FromParent<IWorldElement>(space.Slot);
        searchRequestPanel.FilterList.AddFilter(new DynamicVariableFilter {
            WithinSpace = space,
            QueriedKinds = new HashSet<DynamicVariableElementKind>([
                DynamicVariableElementKind.Component,
                DynamicVariableElementKind.ProtoFlux,
                DynamicVariableElementKind.ButtonInteraction,
            ])
        });
        window.Setup(world);
        if (ResoniteModularSearch.AutoLaunchSmartSearch) {
            searchRequestPanel.RunSearch();
        }
    }

    public static void SearchFrom(World world, Slot slot) {
        var window = new SearchWindow(world);
        var searchRequestPanel = window.SearchRequestPanel;
        searchRequestPanel.Source = new FromParent<IWorldElement>(slot);
        window.Setup(world);
    }

    public static void SearchDynamicVariable(World world, AbstractedDynamicVariableElement abstractedVariable) {
        var window = new SearchWindow(world);
        var searchRequestPanel = window.SearchRequestPanel;
        searchRequestPanel.Source = new FromParent<IWorldElement>(abstractedVariable.Space?.Slot ?? world.RootSlot);
        searchRequestPanel.FilterList.AddFilter(new DynamicVariableFilter {
            OfType = abstractedVariable.Type,
            WithinSpace = abstractedVariable.Space,
            SpaceNamePattern = RegexHelper.TryCompileExactMatch(abstractedVariable.SpaceName),
            VariableNamePattern = RegexHelper.TryCompileExactMatch(abstractedVariable.VariableName),
            QueriedKinds = new HashSet<DynamicVariableElementKind>([
                DynamicVariableElementKind.Component,
                DynamicVariableElementKind.ProtoFlux,
                DynamicVariableElementKind.ButtonInteraction,
            ])
        });
        window.Setup(world);
        if (ResoniteModularSearch.AutoLaunchSmartSearch) {
            searchRequestPanel.RunSearch();
        }
    }

    public static void SearchReferencesTo(World world, IWorldElement reference) {
        var window = new SearchWindow(world);
        var searchRequestPanel = window.SearchRequestPanel;
        searchRequestPanel.FilterList.AddFilter(new ReferenceFilter {
            SearchFor = reference
        });
        window.Setup(world);
        if (ResoniteModularSearch.AutoLaunchSmartSearch) {
            searchRequestPanel.RunSearch();
        }
    }

    public static void SearchForValue<T>(World world, T value) {
        var window = new SearchWindow(world);
        var searchRequestPanel = window.SearchRequestPanel;
        searchRequestPanel.FilterList.AddFilter(new ValueFilter<T> {
            SearchFor = value
        });
        window.Setup(world);
        if (ResoniteModularSearch.AutoLaunchSmartSearch) {
            searchRequestPanel.RunSearch();
        }
    }

    public static void SearchDynamicImpulse(World world, AbstractedDynamicImpulseElement abstractedDynamicImpulse) {
        var window = new SearchWindow(world);
        var searchRequestPanel = window.SearchRequestPanel;
        searchRequestPanel.Source = new FromParent<IWorldElement>(world.RootSlot);
        searchRequestPanel.FilterList.AddFilter(new DynamicImpulseFilter {
            OfType = abstractedDynamicImpulse.Type,
            TagPattern = RegexHelper.TryCompileAnyExactMatch(abstractedDynamicImpulse.TagFields.Select(f => f.Value)),
            IsAsync = abstractedDynamicImpulse.IsAsync,
            WithValue = abstractedDynamicImpulse.WithValue
        });
        window.Setup(world);
        if (ResoniteModularSearch.AutoLaunchSmartSearch) {
            searchRequestPanel.RunSearch();
        }
    }
}
