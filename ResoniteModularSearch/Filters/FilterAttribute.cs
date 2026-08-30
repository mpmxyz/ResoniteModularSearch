namespace ResoniteModularSearch.Filters;

[AttributeUsage(AttributeTargets.Class)]
public class FilterAttribute : Attribute {
    public string Name { get; }

    public FilterAttribute(string name) {
        Name = name;
    }
}
