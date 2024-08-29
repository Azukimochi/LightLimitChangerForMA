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

    public Parameter(T value) : this() => (InitialValue, OverrideValue) = (ToFloatValue(value), ToFloatValue(value));

    /// <summary>
    /// アニメーション用の初期値
    /// </summary>
    public float InitialValue;

    /// <summary>
    /// 上書き用の値
    /// </summary>
    public float OverrideValue;

    private static float ToFloatValue(T value)
    {
        if (typeof(T) == typeof(float))
            return (float)(object)value;

        if (typeof(T) == typeof(int))
            return (int)(object)value;

        if (typeof(T) == typeof(bool))
            return (bool)(object)value ? 1 : 0;

        return float.NaN;
    }

    public static implicit operator Parameter<T>(T value) => new(value);
}
