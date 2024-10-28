namespace io.github.azukimochi;

[Serializable]
public sealed class LilToonSettings : ISettings
{
    string ISettings.ParameterPrefix => "LilToon";

    string ISettings.DisplayName => "LilToon";

    /// <summary>
    /// 影色への環境光影響度
    /// </summary>
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_ShadowEnvStrength")]
    [Range(0, 1)]
    public Parameter<float> ShadowEnvStrength = 0;

    /// <summary>
    /// 頂点ライトの強度
    /// </summary>
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_VertexLightStrength")]
    [Range(0, 1)]
    public Parameter<float> VertexLightStrength = 0;
}
