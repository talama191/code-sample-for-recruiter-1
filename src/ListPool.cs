namespace SpatialStackingStore;

/// <summary>Static pool of reusable lists. Rent with <see cref="Get"/>, return with <see cref="Release"/> — keeps hot paths allocation-free.</summary>
public static class ListPool<T>
{
    private static readonly Stack<List<T>> Free = new();

    public static void Get(ref List<T> list) =>
        list = Free.Count > 0 ? Free.Pop() : new List<T>();

    public static void Release(ref List<T> list)
    {
        list.Clear();
        Free.Push(list);
        list = null!;
    }
}
