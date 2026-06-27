namespace SpatialStackingStore;

/// <summary>Reuses list instances to keep query hot paths allocation-free.</summary>
public sealed class ListPool<T>
{
    private readonly Stack<List<T>> _free = new();

    public List<T> Rent() => _free.Count > 0 ? _free.Pop() : new List<T>();

    public void Return(List<T> list)
    {
        list.Clear();
        _free.Push(list);
    }

    public Scope RentScope(out List<T> list)
    {
        list = Rent();
        return new Scope(this, list);
    }

    public readonly struct Scope : IDisposable
    {
        private readonly ListPool<T> _owner;
        private readonly List<T> _list;

        internal Scope(ListPool<T> owner, List<T> list)
        {
            _owner = owner;
            _list = list;
        }

        public void Dispose() => _owner.Return(_list);
    }
}
