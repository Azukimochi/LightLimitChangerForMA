using System.Collections.Generic;

namespace io.github.azukimochi;

[Serializable]
public abstract class Parameter 
{
    /// <summary>
    /// マテリアルの設定を上書きする
    /// </summary>
    public bool Enable = true;

    /// <summary>
    /// アニメーション・メニューを生成する
    /// </summary>
    public bool IsAnimated = true;

    /// <summary>
    /// 値をセーブする
    /// </summary>
    public bool Saved = true;

    /// <summary>
    /// 値を同期する
    /// </summary>
    public bool Synced = true;

    /// <summary>
    /// 設定値をfloatで取得する
    /// </summary>
    public abstract IEnumerable<float> GetValues();
}

[Serializable]
public sealed class Parameter<T> : Parameter
{
    public Parameter() { }

    public Parameter(T value) : this() => Value = value;

    public T Value;

    public override IEnumerable<float> GetValues()
    {
        // switch is not JIT friendly... 🥺
        if (typeof(T) == typeof(float))
        {
            yield return (float)(object)Value;
        }
        else if (typeof(T) == typeof(int))
        {
            yield return (int)(object)Value;
        }
        else if (typeof(T) == typeof(bool))
        {
            yield return (bool)(object)Value ? 1 : 0;
        }
        else if (typeof(T) == typeof(Color))
        {
            var color = (Color)(object)Value;
            yield return color.r;
            yield return color.g;
            yield return color.b;
            yield return color.a;
        }
    }

    public static implicit operator Parameter<T>(T value) => new(value);
}
