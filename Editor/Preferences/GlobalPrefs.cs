namespace io.github.azukimochi
{
    [FilePath(Preferences.PathRoot + nameof(GlobalPrefs), FilePathAttribute.Location.PreferencesFolder)]
    internal sealed class GlobalPrefs : BasePrefs<GlobalPrefs> { }
}