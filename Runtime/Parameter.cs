namespace io.github.azukimochi;

[Serializable]
public abstract class Parameter 
{
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

    public Parameter(T value, bool enable = true) : this() 
        => (InitialValue, OverrideValue, Enable) = (value, value, enable);

    /// <summary>
    /// アニメーション用の初期値
    /// </summary>
    public T InitialValue;

    /// <summary>
    /// 上書き用の値
    /// </summary>
    public T OverrideValue;

    public static implicit operator Parameter<T>(T value) => new(value);
    public static implicit operator Parameter<T>((T, bool) value) => new(value.Item1, enable: value.Item2);
}
