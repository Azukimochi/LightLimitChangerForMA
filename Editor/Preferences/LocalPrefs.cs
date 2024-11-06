namespace io.github.azukimochi
{
    [FilePath("ProjectSettings/" + Preferences.PathRoot + nameof(LocalPrefs), FilePathAttribute.Location.ProjectFolder)]
    internal sealed class LocalPrefs : BasePrefs<LocalPrefs> 
    {
        public bool AdvancedMode;
        public bool ShowDescription;
    }
}