namespace SpatialStackingStore;

/// <summary>A reusable per-layer arrangement: the slots a package fills, plus the layer height.</summary>
public sealed class LayerPattern
{
    private readonly (int X, int Y)[] _slots;

    public IReadOnlyList<(int X, int Y)> Slots => _slots;

    public int Capacity => _slots.Length;

    public int Height { get; }

    public LayerPattern((int X, int Y)[] slots, int height)
    {
        _slots = slots;
        Height = height;
    }
}
