namespace SpatialStackingStore;

/// <summary>Shelf-packs a uniform package into a layer pattern within a fixed footprint.</summary>
public sealed class LayerPacker
{
    private readonly int _footprintWidth;
    private readonly int _footprintLength;
    private readonly int _minLayerHeight;

    public LayerPacker(int footprintWidth, int footprintLength, int minLayerHeight)
    {
        _footprintWidth = footprintWidth;
        _footprintLength = footprintLength;
        _minLayerHeight = minLayerHeight;
    }

    public LayerPattern Pack(PackageSize package)
    {
        var slots = new List<(int X, int Y)>();
        for (int y = 0; y + package.Length <= _footprintLength; y += package.Length)
        {
            for (int x = 0; x + package.Width <= _footprintWidth; x += package.Width)
            {
                slots.Add((x, y));
            }
        }

        int height = Math.Max(package.Height, _minLayerHeight);
        return new LayerPattern(slots.ToArray(), height);
    }
}
