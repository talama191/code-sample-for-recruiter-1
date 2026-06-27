using System.Diagnostics;
using SpatialStackingStore;

const int footprintWidth = 10;
const int footprintLength = 10;
const int minLayerHeight = 3;

var storage = new LayeredStorage(footprintWidth, footprintLength, minLayerHeight);

int layersViaEvent = 0;
storage.LayerStarted += _ => layersViaEvent++;

var package = new PackageSize(2, 2, 3);
const int count = 10_000;

var sw = Stopwatch.StartNew();
for (int id = 0; id < count; id++)
    storage.Place(new Package(id, package));
sw.Stop();

Console.WriteLine($"Placed {storage.PackageCount:N0} packages of {package.Width}x{package.Length}x{package.Height}:");
Console.WriteLine($"  layers        : {storage.LayerCount}  (counted via event: {layersViaEvent})");
Console.WriteLine($"  used height   : {storage.UsedHeight}");
Console.WriteLine($"  pattern calcs : {storage.Cache.Misses} computed / {storage.Cache.Hits} reused");
Console.WriteLine($"  time          : {sw.Elapsed.TotalMilliseconds:F2} ms\n");

var other = new PackageSize(5, 2, 4);
for (int id = 0; id < 1_000; id++)
    storage.Place(new Package(count + id, other));

Console.WriteLine($"After a second size, the cache holds {storage.Cache.Count} patterns " +
                  $"({storage.Cache.Misses} computed in total).\n");

VerifyInBounds(storage, footprintWidth, footprintLength);
Console.WriteLine("Self-check passed: every placement is within the footprint.");

static void VerifyInBounds(LayeredStorage storage, int width, int length)
{
    List<PackagePlacement> all = storage.RentPlacements();
    try
    {
        foreach (PackagePlacement p in all)
        {
            if (p.X < 0 || p.Y < 0 || p.Z < 0 || p.X >= width || p.Y >= length)
                throw new InvalidOperationException($"Placement out of bounds: {p}.");
        }
    }
    finally
    {
        storage.ReturnPlacements(all);
    }
}
