namespace io.github.azukimochi;

[Serializable]
[MenuIcon(Icons.Poiyomi)]
[SettingOptions(id: "poiyomi", displayName: "Poiyomi", parameterPrefix: "Poiyomi")]
public sealed class PoiyomiSettings : ISettings
{
    string ISettings.ParameterPrefix => "Poiyomi";

    string ISettings.DisplayName => "Poiyomi";
}
