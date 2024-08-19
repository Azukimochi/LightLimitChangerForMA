namespace io.github.azukimochi;

[Serializable]
public sealed class LilToonSettings : ISettings 
{
    /// <summary>
    /// 影色への環境光影響度
    /// </summary>
    [ShaderFeature(SupportedShaders.LilToon)]
    public Parameter<float> ShadowEnvStrength = new(0) { Range = new(0, 1) };

    /// <summary>
    /// 頂点ライトの強度
    /// </summary>
    [ShaderFeature(SupportedShaders.LilToon)]
    public Parameter<float> VertexLightStrength = new(0) { Range = new(0, 1) };
}
