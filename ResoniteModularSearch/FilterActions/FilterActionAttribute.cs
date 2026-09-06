namespace ResoniteModularSearch.FilterActions;

[AttributeUsage(AttributeTargets.Class)]
public class FilterActionAttribute : Attribute {
    public string Name { get; }

    public FilterActionAttribute(string name) {
        Name = name;
    }
}
