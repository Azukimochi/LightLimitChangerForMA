namespace io.github.azukimochi;

/// <summary>
/// 上限と下限を設定できるスライダーで描画する Vector2のみ対応
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
internal sealed class MinMaxSliderAttribute : PropertyAttribute
{
    public MinMaxSliderAttribute(float min, float max) => (Min, Max) = (min, max);

    public float Min { get; }

    public float Max { get; }
}
