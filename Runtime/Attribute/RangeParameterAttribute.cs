namespace io.github.azukimochi;

/// <summary>
/// 他のフィールドによって範囲を限定する
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
internal sealed class RangeParameterAttribute : PropertyAttribute
{
    /// <param name="parameterName">対象Vector2のフィールド名</param>
    public RangeParameterAttribute(string parameterName) => ParameterName = parameterName;

    public string ParameterName { get; }
}
