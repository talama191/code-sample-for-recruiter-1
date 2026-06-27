namespace SpatialStackingStore;

/// <summary>Observer list backing low-overhead events; invokes over a reused snapshot.</summary>
public sealed class CallbackList<T>
{
    private readonly List<Action<T>> _callbacks = new();
    private Action<T>[] _buffer = Array.Empty<Action<T>>();

    public int Count => _callbacks.Count;

    public void Add(Action<T> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _callbacks.Add(callback);
    }

    public bool Remove(Action<T> callback) => _callbacks.Remove(callback);

    public void Invoke(T argument)
    {
        int count = _callbacks.Count;
        if (count == 0) return;

        if (_buffer.Length < count)
            _buffer = new Action<T>[count];

        _callbacks.CopyTo(_buffer, 0);
        for (int i = 0; i < count; i++)
            _buffer[i].Invoke(argument);
    }
}
