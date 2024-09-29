namespace io.github.azukimochi;

/// <summary>
/// シェーダー固有の機能のマーカー
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
internal sealed class ShaderFeatureAttribute : PropertyAttribute
{
    /// <param name="names">プロセサーのID</param>
    public ShaderFeatureAttribute(params string[] names) => QualifiedNames = names;

    public string[] QualifiedNames { get; }
}
