namespace io.github.azukimochi
{
    [FilePath(Preferences.PathRoot + nameof(LocalPrefs), FilePathAttribute.Location.ProjectFolder)]
    internal sealed class LocalPrefs : BasePrefs<LocalPrefs> { }
}