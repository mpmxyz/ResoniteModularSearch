# ResoniteModularSearch

[![Auto-Release](https://github.com/mpmxyz/ResoniteModularSearch/actions/workflows/build-release.yml/badge.svg)](https://github.com/mpmxyz/ResoniteModularSearch/actions/workflows/build-release.yml) [![Check for Resonite Updates](https://github.com/mpmxyz/ResoniteModularSearch/actions/workflows/check-for-resonite-updates.yml/badge.svg)](https://github.com/mpmxyz/ResoniteModularSearch/actions/workflows/check-for-resonite-updates.yml)

This mod for [Resonite](https://resonite.com/) allows searching and editing the world hierarchy based on composable search filters.

The mod itself is modular and it is intended to be extensible via a yet to be published specification. 

Available filters include but are not limited to:
- Values and References
- Regular Expressions
- Dynamic Impulses
- Dynamic Variables

It is possible to compose more complex searches via subqueries.
For example you can find all elements:
- `of type Slot`
- `containing a MeshRenderer`
  - `with a SyncElement` (basic type of a property)
	- `referencing a StaticMesh`

## Installation
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader)!
1. Place [ResoniteModularSearch.dll](https://github.com/mpmxyz/ResoniteModularSearch/releases/latest/download/ResoniteModularSearch.dll) into your `rml_mods` folder! This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a default install. You can create it if it's missing, or if you launch the game once with ResoniteModLoader installed it will create this folder for you.
1. Start the game! If you want to verify that the mod is working you can try to open a search window as demonstrated in the next section.

## Usage / Screenshots

Demo video: https://youtu.be/Zx96oPzvstE

1. The mod can only be interacted with when one has builder permissions within a session.
2. Equip Resonite's `DevTool`! (default or custom)
3. There are multiple ways to create a search window:
- Context Menu -> `Create New...` -> `Search & Replace Window`
- If enabled in the settings (default: yes):
  - Context Menu -> `Search from Root`
  - Grabbing a slot -> Context Menu -> `Search from ...`
  - Grabbing a reference -> Context Menu -> `Search for references to ...`
  - Grabbing a value -> Context Menu -> `Search for occurrences of ...`
4. The search can now be customized:
- At the top you can select the object the search starts from (A, `Search Root`) and how many slots deep the search should progress (B, `Search Depth` with -1 as "unlimited" and 0 as "no child slots"):<br/>
  ![search root and max. depth](Screenshots/SearchSource_annotated.png)
- Next is the list of filters. You can add a new filter by clicking the button `Add Filter...` (A):<br/>
  ![Button: "Add Filter"](Screenshots/AddFilter_annotated.png)
- This will open a window where you can select a filter (A). Some filters have type arguments (B):<br/>
  ![Filter selection with some filters that have type arguments"](Screenshots/FilterSelector_annotated.png)
- Filters can have a number of options (B) that change how a candidate element is "judged".
  Each filter is applied in the order they appear in the filter list. The resulting action (C) is chosen depending on if the candiate element matches the filter or not:<br/>
  The action can be to `Continue` with the next filter, to `Discard` the element from the search result or to `Accept` it without checking other filters. If no final decision is made until the end of the list a default decision is used (E).<br/>
  As the order between filters is relevant it can by changed using the arrows on the left (D).<br/>
  Some filters offer actions (F) that can be run after a search.<br/>
  ![A filter list with a single dynamic variable filter that searches for dynamic variables with a space name of "Avatar"](Screenshots/Filter_annotated.png)
5. Next, run the actual search:
- Click the `Search` (A) button on the bottom of the search configuration panel! A quick summary (B) of the search (or recently run actions) will be displayed below:
  ![Search button and status text](Screenshots/SearchButton_annotated.png)
- The search result looks very similar to a normal inspector panel with the following differences:
  - The hierarchy view (A) only shows slots that contain at least one search result.
    - The slot's names are prefixed with the number of search results `(<# within slot>/<# within slot or children>)`
  - The worker inspector on the right shows only the elements that are part of the search result.
    This can be further narrowed down to a sub-hierarchy or single slot by clicking on an entry within the hierarchy or using the options at the top (B).
    - Slots, components or users are displayed with all their properties.
    - Additional information (D) is displayed depending on the filter. (i.e. the dynamic variable name of `DynamicVariableInput`)
  A major feature is the ability to edit search results. This can be restricted by unchecking the checkbox (C) in front of a result.
  ![SearchResult](Screenshots/SearchResult_annotated.png)

## Included Filters
This is a mini-documentation on the included filters.

It is possible to create your own filters and actions by including implementations of `IFilter` (with no argument constructor) and `IFilterAction` within another mod. The API may be subject to change, though. Check the implementation of [`ValueFilter`](ResoniteModularSearch/Filters/ValueFilter.cs) if you want to see an example!

### Simple Filters
#### [`Destroyable`](ResoniteModularSearch/Filters/DestroyableFilter.cs)
This filter only matches elements that can be destroyed. (implementing [`IDestroyable`](https://wiki.resonite.com/Type:IDestroyable))

It features a single action that can destroy all selected search results.

#### [`Dynamic Impulse`](ResoniteModularSearch/Filters/DynamicImpulses/DynamicImpulseFilter.cs)
This filter allows searching for components and ProtoFlux nodes related to [dynamic impulses](https://wiki.resonite.com/Dynamic_impulses).
There is partial support for constant values fed into `Tag` inputs of ProtoFlux nodes.

The following option are available:

|Option|Details|
|-|-|
|Of type|if non-null restricts results to triggers/receivers that transport a value of the given type|
|Tag pattern|a regular expression that must match the tag of the dynamic impulse if non-null|
|Include Button interactions/ProtoFlux nodes|allows restricting the type of results|
|(sync/async)|allows choosing if only sync or async impulses are matched|
|(with/without values)|allows choosing if matched impulses are supposed to transport a value|
|(triggers/receivers)|allows choosing if only trigger or only receivers are matched|
|(enabled/disabled)|allows restricting match of receivers to ones (not) reachable by triggers with `Exclude Disabled` set to `true`|

A single action is offered:

|Action|Details|
|-|-|
|Replace tag|uses the regular expression `Tag pattern` to apply a replacement using `Replace tag with` on all selected search result's `Tag` property or input|

#### [`Dynamic Variable`](ResoniteModularSearch/Filters/DynamicVariables/DynamicVariableFilter.cs)
This filter allows searching for components and ProtoFlux nodes related to [dynamic variables](https://wiki.resonite.com/Dynamic_variables).
There is partial support for constant values fed into `Name` inputs of ProtoFlux nodes.

Read/write nodes are speculatively "linked" to dynamic variable spaces depending on their inputs.

The following option are available:

|Option|Details|
|-|-|
|Of type|if non-null restricts results to elements that relate to a variable of the given type|
|Search for (linked/unlined)|allows restricting search to linked/unlinked variables/nodes only|
|Within space|if non-null restricts results to elements that are the given dynamic variable space or are interacting with/linked to it|
|Space name pattern|a regular expression that is matched on the (linked/expected) spaces' name if the pattern is non-null|
|Matched space name|selects the source for the space name pattern in case of variables (name of linked space vs. name specified in variable name)|
|Variable name pattern|a regular expression that is matched on the variable name if the pattern is non-null (ignores space-prefixes)|
|Include spaces/components/ProtoFlux nodes/Button interactions|allows restricting the type of results|

Multiple actions are offered on selected search results:

|Action|Details|
|-|-|
|Replace space name|uses the regular expression `Space name pattern` to apply a replacement using `Replace space name with` on all selected search result's `Name` property or input (only affects space name prefix on variables)|
|Replace variable name|uses the regular expression `Variable name pattern` to apply a replacement using `Replace variable name with` on all selected search result's `Name` property or input (ignores space name prefixes)|
|Add space name|forces the explicit use of dynamic variable spaces's name in variable names|
|Remove space name|removes space name prefixes from all variables or inputs as long as the result links to the same space|

#### [`Is Sync Member`](ResoniteModularSearch/Filters/SyncMemberFilter.cs)
This filter only matches `ISyncMember`s and offers to match its name with the regular expression `Name Pattern`. (ignored if null)

#### [`Is Type`](ResoniteModularSearch/Filters/ElementTypeFilter.cs)
This filter only matches elements that are of a given type. (with an option to allow subtypes)

#### [`Filter List`](ResoniteModularSearch/Filters/FilterList.cs)
This filter allows combining multiple filters and is actually the root filter set-up on all search windows.
The section [Usage / Screenshots](#usage-screenshots) contains a detailed description on how to use them.

#### [`Hierarchy`](ResoniteModularSearch/Filters/HierarchyFilter.cs)
This filter allows checking if the search result is a `Parent of` / `Child of` a given element. (ignored if null)

Enabling the option `Include self` makes it also match the given element(s).

#### [`Reference`](ResoniteModularSearch/Filters/ReferenceFilter.cs) / [`Value`](ResoniteModularSearch/Filters/ValueFilter.cs)
Both filters allow to `Search for` properties with an exact match of a reference or value. The matched references / values can be `Replace`d with the reference / value `Replace with`.

#### [`Regular Expression`](ResoniteModularSearch/Filters/RegexFilter.cs)
This filter allows to search `string` properties that match the a given (non-null) regular expression.

The `Replace` action can be applied on all selected search results.

### Subqueries
Subqueries are special because they start a _nested_ search for each candidate element.

A subquery filter only _matches_ when it finds at least one search result within the nested search.

Nested search results are appended as additional information to the search results.

#### [`Containing Element`](ResoniteModularSearch/Filters/SubQueries/ContainingElementFilter.cs)
This subquery goes through all elements contained within the matched element.
The type argument allows efficiently restricting the number of elements checked by this subquery.

A `Max depth` other than 0 allows searching through child slots and their contained 
elements. The number limits how deep the search goes:

|Value|Interpretation|
|-|-|
|-1|unlimited|
|0|no children|
|1|direct children|
|2|direct children and their direct children|

#### [`Has Sync Member`](ResoniteModularSearch/Filters/SubQueries/HasSyncMemberFilter.cs)
This subquery goes through all sync members that are of or a subtype of the type argument.

#### [`Referencing Element`](ResoniteModularSearch/Filters/SubQueries/ReferencingElementFilter.cs)
This subquery matches all reference properties and tries to apply the contained filters on the target of the property.

#### [`Within Element`](ResoniteModularSearch/Filters/SubQueries/WithinElementFilter.cs)
This subquery tries to find a parent of the matched element. `Max Depth` limits how many times the subquery goes from a slot to its parent slot. (-1=unlimited)
