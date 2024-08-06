namespace io.github.azukimochi;

[Serializable]
public sealed class Parameter<T>
{
    public T Value;

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

    public static implicit operator Parameter<T>(T value) => new() { Value = value };
    public static explicit operator T(Parameter<T> parameter) => parameter.Value;
}