namespace io.github.azukimochi;

internal sealed class Unizy<T> where T : Object
{
    private readonly Func<T> factory;
    private T value = null;

    public Unizy(Func<T> factory)
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

    public static implicit operator T(Unizy<T> value) => value.Value;
}
