using System.Drawing;

namespace io.github.azukimochi;

[Serializable]
[MenuIcon(Icons.LilToon)]
[SettingOptions(id: "lilToon", displayName: "LilToon", parameterPrefix: "LilToon")]
public sealed class LilToonSettings : ISettings
{
    /// <summary>
    /// 影色への環境光影響度
    /// </summary>
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_ShadowEnvStrength")]
    [MenuIcon(Icons.ShadowEnvStrength)]
    [Range(0, 1)]
    public Parameter<float> ShadowEnvStrength = 0;

    /// <summary>
    /// 頂点ライトの強度
    /// </summary>
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_VertexLightStrength")]
    [MenuIcon(Icons.VertexLightStrength)]
    [Range(0, 1)]
    public Parameter<float> VertexLightStrength = 0;
}
