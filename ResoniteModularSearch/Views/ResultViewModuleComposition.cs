using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;
using ResoniteModularSearch.Filters.DynamicImpulses;
using ResoniteModularSearch.Filters.DynamicVariables;
using ResoniteModularSearch.Filters.SubQueries;
using ResoniteModularSearch.Search;

namespace ResoniteModularSearch.Views;
public class ResultViewModuleComposition {
    public static readonly ResultViewModuleComposition Default = new();

    private readonly HashSet<IResultViewModule> modules = [];
    private IReadOnlyList<IResultViewAppender>? appenders = null;
    private IReadOnlyList<IResultViewMask>? masks = null;

    public IReadOnlyList<IResultViewAppender> Appenders {
        get {
            appenders ??= modules
                .OfType<IResultViewAppender>()
                .OrderBy(item => item.AppendOffset)
                .ToImmutableList();
            return appenders;
        }
    }
    public IReadOnlyList<IResultViewMask> Masks {
        get {
            masks ??= modules
                .OfType<IResultViewMask>()
                .OrderBy(item => item.MaskOffset)
                .ToImmutableList();
            return masks;
        }
    }

    static ResultViewModuleComposition() {
        //TODO: auto-detect modules
        Default.RegisterModule(new DefaultViewAppender());
        Default.RegisterModule(new DynamicImpulseViewModule());
        Default.RegisterModule(new DynamicVariableViewModule());
        Default.RegisterModule(new SubQueryViewModule());
    }

    public void RegisterModule(IResultViewAppender module) {
        if (modules.Add(module)) {
            //TODO: setup auto-reset of sorted list of masks/appenders
            appenders = null;
            masks = null;
        }
    }

    public bool TryGetMaskSource(IWorldElement element, SearchResult result, IWorker? partOf, [NotNullWhen(true)] out IResultViewMask? shadowingModule) {
        shadowingModule = Masks.FirstOrDefault(module => module.MasksElement(element, result, partOf));
        return shadowingModule != null;
    }

    public int BuildResultItemView(UIBuilder builder, IWorldElement element, SearchResult result) {
        int nLines = 0;
        var rootSlot = builder.HorizontalLayout(StyleHelpers.DEFAULT_SPACING).Slot;
        void DestroyRoot(IDestroyable destroyed) {
            if (!rootSlot.IsDestroyed){
                rootSlot.Destroy();
            }
        }
        var destroyable = element as IDestroyable ?? element.FindNearestParent<IDestroyable>();
        if (destroyable != null) {
            destroyable.Destroyed += DestroyRoot;
            rootSlot.Destroyed += (destroyed) => {
                if (!destroyable.IsDestroyed) {
                    destroyable.Destroyed -= DestroyRoot;
                }
            };
        }
        {
            builder.PushStyle();
            builder.Style.Width = StyleHelpers.DEFAULT_MIN_SIZE;
            builder.Style.ForceExpandWidth = true;
            builder.Style.ForceExpandHeight = false;
            builder.VerticalLayout();
            builder.PopStyle();
            builder.PushStyle();
            builder.Style.Height = StyleHelpers.DEFAULT_MIN_SIZE;
            builder.Checkbox(result.SelectedResults.Contains(element)).State.OnValueChange += (state) => result.SetSelected(element, state.Value);
            //TODO: add way to update UI state from SearchResult (using result values?)
            builder.PopStyle();
            builder.NestOut();
        }
        {
            builder.PushStyle();
            builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING);
            builder.PopStyle();
            foreach (var module in Appenders) {
                nLines += module.TryAppendUI(builder, element, result, this);
            }
            builder.NestOut();
        }
        builder.NestOut();
        return nLines;
    }

    public void Setup(UIBuilder builder) {
        //TODO: build list with modules that can be reordered
        //      -> to solve: There are two orders. (append, mask)
        throw new NotImplementedException();
    }
}
