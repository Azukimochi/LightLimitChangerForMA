namespace io.github.azukimochi;

/// <summary>
/// 汎用コントロールのマーカー
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
internal sealed class GeneralControlAttribute : Attribute
{
    public GeneralControlAttribute(GeneralControlType type) => Type = type;

    public GeneralControlType Type { get; }
}
