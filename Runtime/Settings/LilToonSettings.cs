namespace io.github.azukimochi;

[Serializable]
[ShaderFeature(BuiltinSupportedShaders.LilToon)]
public sealed partial class LilToonSettings : ISettings
{
    string ISettings.ParameterPrefix => "LilToon";

    string ISettings.DisplayName => "LilToon";

    /// <summary>
    /// 影色への環境光影響度
    /// </summary>
    [Range(0, 1)]
    public Parameter<float> ShadowEnvStrength = 0;

    /// <summary>
    /// 頂点ライトの強度
    /// </summary>
    [Range(0, 1)]
    public Parameter<float> VertexLightStrength = 0;

    public BacklightSettings Backlight;
}

partial class LilToonSettings
{
    [Serializable]
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    public sealed class BacklightSettings : ISettings
    {
        public string ParameterPrefix => "LilToon/Backlight";

        public string DisplayName => "Backlight";

        /// <summary>
        /// 色
        /// </summary>
        public Color Color = Color.white;

        [Range(0, 1)]
        public Parameter<float> Opacity = 1f;

        /// <summary>
        /// メインカラーの強度
        /// </summary>
        [Range(0, 1)]
        public Parameter<float> MainStrength = 0;

        public Parameter<bool> ReceiveShadow = true;

        public Parameter<bool> BackfaceMask = true;

        [Range(0, 1)]
        public Parameter<float> NormalStrength = 1;

        [Range(0, 1)]
        public Parameter<float> Border = 0.65f;

        [Range(0, 1)]
        public Parameter<float> Blur = 0.05f;

        [RangeParameter(nameof(DirectivityRange))]
        public Parameter<float> Directivity = 5f;

        [MinMaxSlider(0, 5)]
        public Vector2 DirectivityRange = new(0, 5);

        [Range(0, 1)]
        public Parameter<float> ViewStrength = 0.05f;
    }
}