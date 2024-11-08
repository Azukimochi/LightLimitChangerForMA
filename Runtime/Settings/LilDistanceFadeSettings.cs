namespace io.github.azukimochi;

[Serializable]
[MenuIcon(Icons.DistanceFade)]
[SettingOptions(id: "liltoon-distancefade", displayName: "DistanceFade", parameterPrefix: "LilToonDistanceFade")]
public class LilDistanceFadeSettings : ISettings
{
    /// <summary>
    /// 距離フェード
    /// </summary>
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [VectorField(VectorField.X)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_DistanceFade.x")]
    [MenuIcon(Icons.DistanceFadeX)]
    [Range(0, 1)]
    public Parameter<float> Start = new (0.1f) {Enable = false, IsAnimated = false};
    
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [VectorField(VectorField.Y)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_DistanceFade.y")]
    [MenuIcon(Icons.DistanceFadeY)]
    [Range(0, 1)]
    public Parameter<float> End = new (0.01f) {Enable = false, IsAnimated = false};
    
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [VectorField(VectorField.Z)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_DistanceFade.z")]
    [MenuIcon(Icons.DistanceFadeZ)]
    [Range(0, 1)]
    public Parameter<float> Strength = new (1.0f) {Enable = false, IsAnimated = false};
    
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [VectorField(VectorField.W)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_DistanceFade.w")]
    [MenuIcon(Icons.DistanceFadeW)]
    public Parameter<bool> BackfaceForceShadow = new (false) {Enable = false, IsAnimated = false};
}
