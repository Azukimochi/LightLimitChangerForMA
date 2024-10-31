namespace io.github.azukimochi;

[Serializable]
[MenuIcon(Icons.Poiyomi)]
public sealed class PoiyomiSettings : ISettings
{
    string ISettings.ParameterPrefix => "Poiyomi";

    string ISettings.DisplayName => "Poiyomi";
}
