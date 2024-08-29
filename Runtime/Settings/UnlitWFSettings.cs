namespace io.github.azukimochi;

[Serializable]
public sealed class UnlitWFSettings : ISettings
{
    string ISettings.ParameterPrefix => "UnlitWF";

    string ISettings.DisplayName => "UnlitWF";
}
