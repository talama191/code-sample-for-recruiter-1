namespace SpatialStackingStore;

public sealed class Package
{
    public int Id { get; }

    public PackageSize Size { get; }

    public Package(int id, PackageSize size)
    {
        Id = id;
        Size = size;
    }
}
