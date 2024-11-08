namespace io.github.azukimochi;

[Serializable]
[MenuIcon(Icons.Backlight)]
[SettingOptions(id: "liltoon-backlight", displayName: "Backlight", parameterPrefix: "LilToonBacklight")]
public class LilBacklightSettings : ISettings
{
    // _BacklightColor  背面光の色
    // _UseBacklight　ONOFF
    // _BacklightMainStrength メインカラーの強度
    
    // _BacklightBorder 範囲
    // _BacklightBlor　ぼかし
    // _BacklightDirectivity　指向性
    // _BacklightViewStrength 視線方向の影響度　
    
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_UseBacklight")]
    [MenuIcon(Icons.UseBacklight)]
    public Parameter<bool> UseBacklight = new (false) {Enable = false};
    
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_BacklightColor")]
    [MenuIcon(Icons.BacklightColor)]
    public Parameter<Color> BacklightColor = new (Color.white) {Enable = false};
    
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_BacklightMainStrength")]
    [MenuIcon(Icons.BacklightMainStrength)]
    [Range(0, 1)]
    public Parameter<float> BacklightMainStrength = new (0.0f) {Enable = false};
    
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_BacklightBorder")]
    [MenuIcon(Icons.BacklightBorder)]
    [Range(0, 1)]
    public Parameter<float> BacklightBorder = new (0.65f) {Enable = false};
    
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_BacklightBlur")]
    [MenuIcon(Icons.BacklightBlur)]
    [Range(0, 1)]
    public Parameter<float> BacklightBlur = new (0.05f) {Enable = false};
    
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_BacklightDirectivity")]
    [MenuIcon(Icons.BacklightDirectivity)]
    [MinMaxRange(0, 10)]
    public Parameter<float> BacklightDirectivity = new (5.0f) {Enable = false, MinMaxRange = new (0,5)};
    
    [ShaderFeature(BuiltinSupportedShaders.LilToon)]
    [MaterialPropertyName(BuiltinSupportedShaders.LilToon, "_BacklightViewStrength")]
    [MenuIcon(Icons.BacklightViewStrength)]
    [Range(0, 1)]
    public Parameter<float> BacklightViewStrength = new (1.0f) {Enable = false};
}
