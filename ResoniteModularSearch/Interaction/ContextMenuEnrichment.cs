






using Elements.Core;

using FrooxEngine;

using ResoniteModularSearch.Filters.DynamicImpulses;
using ResoniteModularSearch.Filters.DynamicVariables;

namespace ResoniteModularSearch.Interaction;

public static class ContextMenuEnrichment {
    public static void SearchFrom(ContextMenu menu, Slot slot) {
        menu.AddLocalActionItem($"Search from {slot.Name}", null, colorX.Red, delegate (IButton b, ButtonEventData ev) {
            SearchTemplate.SearchFrom(b.World, slot);
        });
    }

    public static void SearchDynamicVariableSpace(ContextMenu menu, DynamicVariableSpace space) {
        menu.AddLocalActionItem($"Search variables in {space.SpaceName.Value}", null, colorX.Red, delegate (IButton b, ButtonEventData ev) {
            SearchTemplate.SearchDynamicVariableSpace(b.World, space);
        });
    }

    public static void SearchDynamicVariable(ContextMenu menu, AbstractedDynamicVariableElement abstractedVariable) {
        menu.AddLocalActionItem($"Search for usages of variable \"{abstractedVariable.VariableName}\"", null, colorX.Red, delegate (IButton b, ButtonEventData ev) {
            SearchTemplate.SearchDynamicVariable(b.World, abstractedVariable);
        });
    }

    public static void SearchReferencesTo(ContextMenu menu, IWorldElement reference) {
        menu.AddLocalActionItem($"Search for references to {reference.Name}", null, colorX.Red, delegate (IButton b, ButtonEventData ev) {
            SearchTemplate.SearchReferencesTo(b.World, reference);
        });
    }

    public static void SearchForValue<T>(ContextMenu menu, T value) {
        menu.AddLocalActionItem($"Search for occurrences of {value}", null, colorX.Red, delegate (IButton b, ButtonEventData ev) {
            SearchTemplate.SearchForValue<T>(b.World, value);
        });
    }

    public static void SearchDynamicImpulse(ContextMenu menu, AbstractedDynamicImpulseElement abstractedDynamicImpulse) {
        menu.AddLocalActionItem($"Search for usages of dynamic impulse", null, colorX.Red, delegate (IButton b, ButtonEventData ev) {
            SearchTemplate.SearchDynamicImpulse(b.World, abstractedDynamicImpulse);
        });
    }
}
