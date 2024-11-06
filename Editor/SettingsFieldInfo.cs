using System.Reflection;

namespace io.github.azukimochi;

internal sealed class SettingsFieldInfo<TSettings> where TSettings : ISettings
{
    public static SettingOptionsAttribute Options { get; }

    public static string Id { get; }
    public static string DisplayName { get; }
    public static string ParameterPrefix { get; }

    static SettingsFieldInfo()
    {
        Options = typeof(TSettings).GetCustomAttribute<SettingOptionsAttribute>();
        if (Options is null)
        {
            Id = typeof(TSettings).FullName;
            DisplayName = typeof(TSettings).Name;
            ParameterPrefix = typeof(TSettings).Name;
        }
        else
        {
            Id = Options.Id;
            DisplayName = Options.DisplayName;
            ParameterPrefix = Options.ParameterPrefix;
        }
    }
}