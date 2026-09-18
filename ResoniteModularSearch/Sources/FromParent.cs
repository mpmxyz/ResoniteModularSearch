using FrooxEngine;
using FrooxEngine.UIX;

using ResoniteModularSearch.DataModel;

namespace ResoniteModularSearch.Sources;

//TODO: change to multiple search roots
public class FromParent<T>(IWorldElement searchRoot) : ISearchSource where T : IWorldElement {
    private static readonly bool CouldOutputSyncMembers;
    private static readonly bool CouldOutputComponents;
    private static readonly bool CouldOutputContainers;
    private static readonly bool CouldOutputStreams;
    static FromParent() {
        if (typeof(T).IsAssignableTo(typeof(Slot)) || typeof(T).IsAssignableTo(typeof(User))) {
            CouldOutputSyncMembers = false;
            CouldOutputComponents = false;
            CouldOutputContainers = true;
            CouldOutputStreams = false;
        } else if (typeof(T).IsAssignableTo(typeof(Component)) || typeof(T).IsAssignableTo(typeof(UserComponent))) {
            CouldOutputSyncMembers = false;
            CouldOutputComponents = true;
            CouldOutputContainers = false;
            CouldOutputStreams = false;
        } else if (typeof(T).IsAssignableTo(typeof(FrooxEngine.Stream))) {
            CouldOutputSyncMembers = false;
            CouldOutputComponents = false;
            CouldOutputContainers = false;
            CouldOutputStreams = true;
        } else if (typeof(T).IsAssignableTo(typeof(ISyncMember))) {
            CouldOutputSyncMembers = true;
            CouldOutputComponents = false;
            CouldOutputContainers = false;
            CouldOutputStreams = false;
        } else {
            CouldOutputSyncMembers = true;
            CouldOutputComponents = true;
            CouldOutputContainers = true;
            CouldOutputStreams = true;
        }
    }

    public IWorldElement? SearchRoot { get; set; } = searchRoot;

    public bool IncludeSearchRoot { get; set; } = true;
    public bool IncludeSyncMembers { get; set; } = CouldOutputSyncMembers;
    public bool IncludeComponents { get; set; } = CouldOutputComponents;
    public bool IncludeStreams { get; set; } = CouldOutputStreams;
    public bool IncludeContainers { get; set; } = CouldOutputContainers;
    public bool VisitComponents { get; set; } = CouldOutputSyncMembers || CouldOutputComponents;
    public int MaxDepth { get; set; } = -1; //-1 == infinite

    public IEnumerable<IWorldElement> RootElements {
        get {
            if (SearchRoot != null) {
                return [SearchRoot];
            } else {
                return [];
            }
        }
    }

    public void ProcessComponentList(IEnumerable<Component> components, List<IWorldElement> outputList) {
        if (IncludeComponents) {
            outputList.AddRange(components);
        }
        ProcessContainedSyncMembers(components, outputList);
    }
    public void ProcessUserComponentList(IEnumerable<UserComponent> components, List<IWorldElement> outputList) {
        if (IncludeComponents) {
            outputList.AddRange(components);
        }
        ProcessContainedSyncMembers(components, outputList);
    }

    public void ProcessSlotList(IEnumerable<Slot> slots, Func<IWorldElement, bool> mask, int previousDepth, List<IWorldElement> outputList) {
        if (IncludeContainers) {
            outputList.AddRange(slots);
        }
        ProcessContainedSyncMembers(slots, outputList);
        if (VisitComponents) {
            foreach (var slot in slots) {
                if (mask(slot)) {
                    ProcessComponentList(slot.Components, outputList);
                }
            }
        }
        if (previousDepth != MaxDepth) { //implicitly makes MaxDepth==-1 an unrestricted recursion
            foreach (var slot in slots) {
                if (mask(slot)) {
                    ProcessSlotList(slot.Children, mask, previousDepth + 1, outputList);
                }
            }
        }
    }
    public void ProcessUserList(IEnumerable<User> users, List<IWorldElement> outputList) {
        if (IncludeContainers) {
            outputList.AddRange(users);
        }
        ProcessContainedSyncMembers(users, outputList);
        if (VisitComponents) {
            foreach (var user in users) {
                ProcessUserComponentList(user.Components, outputList);
            }
        }
    }
    public void ProcessOtherWorker(Worker worker, List<IWorldElement> outputList) {
        outputList.Add(worker);
        ProcessContainedSyncMembers([worker], outputList);
    }

    public void ProcessContainedSyncMembers<W>(IEnumerable<W> workers, List<IWorldElement> outputList) where W : Worker {
        if (IncludeSyncMembers) {
            foreach (var worker in workers) {
                outputList.AddRange(worker.GetSyncMembers<IWorldElement>());
            }
        }
    }

    public void ProcessRoot(Func<IWorldElement, bool> mask, List<IWorldElement> outputList) {
        if (SearchRoot == null) {
            return;
        }
        if (SearchRoot is Worker worker) {
            switch (worker) {
                case Slot slot:
                    ProcessSlotList([slot], mask, 0, outputList);
                    break;
                case User user:
                    ProcessUserList([user], outputList);
                    break;
                case Component component:
                    ProcessComponentList([component], outputList);
                    break;
                case UserComponent component:
                    ProcessUserComponentList([component], outputList);
                    break;
                default:
                    ProcessOtherWorker(worker, outputList);
                    break;
            }
        } else {
            outputList.Add(SearchRoot);
        }
    }

    public IEnumerable<IWorldElement> GetAllCandidates(Func<IWorldElement, bool> mask) {
        List<IWorldElement> candidateList = [];
        ProcessRoot(mask, candidateList);
        ResoniteModularSearch.Msg($"Candidates: {candidateList.Count()}");
        var results = candidateList.Where((output) => output is T && mask(output));
        if (IncludeSearchRoot) {
            return results;
        } else {
            var searchRoot = SearchRoot;
            return results.Where((output) => output != searchRoot);
        }
    }

    public void Setup(UIBuilder builder) {
        builder.VerticalLayout(StyleHelpers.DEFAULT_SPACING); //TODO: move vertical layout out of filter UI generation -> use super setup
        builder.CreateReferenceEditor("Search Root", SearchRoot, (value) => SearchRoot = value);
        builder.CreateValueEditor("Max Depth (levels of slots)", MaxDepth, (value) => MaxDepth = value);
        //builder.CreateValueEditor("Include search root", IncludeSearchRoot, (value) => IncludeSearchRoot = value);
        builder.NestOut();

        //TODO: more options?
    }
}
