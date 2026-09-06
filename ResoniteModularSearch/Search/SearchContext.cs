using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace ResoniteModularSearch.Search;
public class SearchContext {
    private readonly IDictionary<object, object> content;

    public SearchContext(IDictionary<object, object> content) {
        this.content = content;
    }
    public SearchContext() : this(new Dictionary<object, object>()) {

    }

    public SearchContext ToFrozenContext() {
        return new(content.ToFrozenDictionary());
    }

    private record struct DictionaryKey<V>(object Key) {

    }

    public void SetValue<V>(object key, V value) where V : notnull {
        content.Add(new DictionaryKey<V>(key), value);
    }

    public bool TryGetValue<V>(object key, [MaybeNullWhen(false)] out V value) where V : notnull {
        if (content.TryGetValue(new DictionaryKey<V>(key), out var rawValue)) {
            if (rawValue is V castValue) {
                value = castValue;
                return true;
            }
        }
        value = default;
        return false;
    }
}
