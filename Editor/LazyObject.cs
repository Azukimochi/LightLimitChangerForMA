namespace io.github.azukimochi;

internal sealed class LazyObject<T> where T : Object
{
    private readonly Func<T> factory;
    private T value = null;

    public LazyObject(Func<T> factory)
    {
        this.factory = factory;
    }

    public T Value
    {
        get
        {
            if (value == null)
                value = factory();
            return value;
        }
    }

    public static implicit operator T(LazyObject<T> value) => value.Value;
}