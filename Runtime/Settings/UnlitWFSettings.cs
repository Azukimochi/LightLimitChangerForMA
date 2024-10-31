namespace io.github.azukimochi;

[Serializable]
[MenuIcon(Icons.UnlitWF)]
public sealed class UnlitWFSettings : ISettings
{
    string ISettings.ParameterPrefix => "UnlitWF";

    string ISettings.DisplayName => "UnlitWF";
}
