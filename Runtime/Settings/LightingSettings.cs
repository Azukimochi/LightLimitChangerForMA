namespace io.github.azukimochi;

[Serializable]
public sealed class LightingSettings : ISettings
{
    /// <summary>
    /// 明るさの下限
    /// </summary>
    public Parameter<float> MinLight = 0.05f;

    /// <summary>
    /// 明るさの上限
    /// </summary>
    public Parameter<float> MaxLight = 1.00f;

    /// <summary>
    /// 明るさの下限の範囲
    /// </summary>
    [MinMaxSlider(0, 1)]
    public Vector2 MinLightRange = new(0, 1);

    /// <summary>
    /// 明るさの上限の範囲
    /// </summary>
    [MinMaxSlider(0, 10)]
    public Vector2 MaxLightRange = new(0, 1);

    /// <summary>
    /// 光の色の無視具合
    /// </summary>
    [ShaderFeature(SupportedShaders.LilToon)]
    public Parameter<float> Monochrome = 0;

    /// <summary>
    /// 光の無視具合
    /// </summary>
    [ShaderFeature(SupportedShaders.LilToon)]
    public Parameter<float> Unlit = 0;

    /// <summary>
    /// 影色への環境光影響度
    /// </summary>
    [ShaderFeature(SupportedShaders.LilToon)]
    public Parameter<float> ShadowEnvStrength = 0;

    /// <summary>
    /// 頂点ライトの強度
    /// </summary>
    [ShaderFeature(SupportedShaders.LilToon)]
    public Parameter<float> VertexLightStrength = 0;
}
