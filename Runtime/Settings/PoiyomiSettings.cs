namespace io.github.azukimochi;

[Serializable]
public sealed class PoiyomiSettings : ISettings
{
    string ISettings.ParameterPrefix => "Poiyomi";
}
