namespace io.github.azukimochi;

[Serializable]
public abstract class Parameter 
{
    public Vector2 Range = Vector2.zero;

    /// <summary>
    /// 有効／無効
    /// </summary>
    public bool Enable = true;

    /// <summary>
    /// 値をセーブする
    /// </summary>
    public bool Saved = true;

    /// <summary>
    /// 値を同期する
    /// </summary>
    public bool Synced = true;
}

[Serializable]
public sealed class Parameter<T> : Parameter
{
    public Parameter() { }

    public Parameter(T value) : this() => (InitialValue, OverrideValue) = (value, value);

    public T InitialValue;
    public T OverrideValue;

    public static implicit operator Parameter<T>(T value) => new(value);

    public void Deconstruct(out T initialValue, out T overrideValue) => (initialValue, overrideValue) = (InitialValue, OverrideValue);
}
