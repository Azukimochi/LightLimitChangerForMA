namespace io.github.azukimochi;

[Serializable]
public sealed class GeneralSettings : ISettings
{
    /// <summary>
    /// アニメーションやメニューを生成する
    /// </summary>
    public bool AllowParameterController = true;

    /// <summary>
    /// マテリアルの設定を上書きする
    /// </summary>
    public bool OverwriteMaterialParameters = true;

    /// <summary>
    /// AnchorOverrideとRoot Boneを上書きする（しかしどこに？）
    /// </summary>
    public bool OverwriteMeshSettings = true;

    public LightingSettings Lighting = new();
    public ColorControlSettings ColorControl = new();
    public EmissionControlSettings EmissionControl = new();
}
