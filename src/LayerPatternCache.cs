namespace SpatialStackingStore;

/// <summary>Caches one layer pattern per package size, so repeated layers reuse a single calculation.</summary>
public sealed class LayerPatternCache : IDisposable
{
    private readonly Dictionary<PackageSize, LayerPattern> _patterns = new();

    public int Hits { get; private set; }

    public int Misses { get; private set; }

    public int Count => _patterns.Count;

    public void Dispose() => _patterns.Clear();

    public LayerPattern GetOrCompute(PackageSize size, Func<PackageSize, LayerPattern> compute)
    {
        if (_patterns.TryGetValue(size, out LayerPattern? pattern))
        {
            Hits++;
            return pattern;
        }

        Misses++;
        pattern = compute(size);
        _patterns[size] = pattern;
        return pattern;
    }
}
