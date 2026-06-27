namespace SpatialStackingStore;

/// <summary>Fixed-footprint, unbounded-height storage that fills layer by layer, reusing cached patterns.</summary>
public sealed class LayeredStorage : IDisposable
{
    private readonly LayerPacker _packer;
    private readonly LayerPatternCache _cache = new();
    private readonly List<PackagePlacement> _placements = new();

    private LayerPattern? _currentPattern;
    private PackageSize _currentSize;
    private int _slotIndex;
    private int _currentLayerBaseZ;

    public int Width { get; }

    public int Length { get; }

    public int MinLayerHeight { get; }

    public int LayerCount { get; private set; }

    public int UsedHeight { get; private set; }

    public int PackageCount => _placements.Count;

    public LayerPatternCache Cache => _cache;

    private readonly CallbackList<PackagePlacement> _placedCallbacks = new();

    public event Action<PackagePlacement> Placed
    {
        add => _placedCallbacks.Add(value);
        remove => _placedCallbacks.Remove(value);
    }

    private readonly CallbackList<int> _layerStartedCallbacks = new();

    public event Action<int> LayerStarted
    {
        add => _layerStartedCallbacks.Add(value);
        remove => _layerStartedCallbacks.Remove(value);
    }

    public LayeredStorage(int width, int length, int minLayerHeight)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (minLayerHeight <= 0) throw new ArgumentOutOfRangeException(nameof(minLayerHeight));

        Width = width;
        Length = length;
        MinLayerHeight = minLayerHeight;
        _packer = new LayerPacker(width, length, minLayerHeight);
    }

    public void Dispose()
    {
        _placements.Clear();
        _cache.Dispose();
        _placedCallbacks.Dispose();
        _layerStartedCallbacks.Dispose();
    }

    private bool NeedNewLayer(PackageSize size) =>
        _currentPattern is null || _slotIndex >= _currentPattern.Capacity || size != _currentSize;

    private void OpenLayer(PackageSize size)
    {
        if (_currentPattern is not null)
            _currentLayerBaseZ += _currentPattern.Height; // stack the new layer on top of the last

        _currentPattern = _cache.GetOrCompute(size, _packer.Pack);
        if (_currentPattern.Capacity == 0)
            throw new InvalidOperationException("Package footprint is larger than the storage footprint.");

        _currentSize = size;
        _slotIndex = 0;
        LayerCount++;
        UsedHeight = _currentLayerBaseZ + _currentPattern.Height;
        _layerStartedCallbacks.Invoke(_currentLayerBaseZ);
    }

    public PackagePlacement Place(Package package)
    {
        if (NeedNewLayer(package.Size))
            OpenLayer(package.Size);

        (int x, int y) = _currentPattern!.Slots[_slotIndex];
        _slotIndex++;

        var placement = new PackagePlacement(x, y, _currentLayerBaseZ);
        _placements.Add(placement);
        _placedCallbacks.Invoke(placement);
        return placement;
    }

    public void CopyPlacementsTo(List<PackagePlacement> destination) => destination.AddRange(_placements);
}
