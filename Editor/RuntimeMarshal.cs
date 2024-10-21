
namespace io.github.azukimochi
{
    [InitializeOnLoad]
    internal static class RuntimeMarshal
    {
        static RuntimeMarshal()
        {
            EditorMarshal.GetCurrentVersion = () => LightLimitChanger.Version;
        }
    }
}
