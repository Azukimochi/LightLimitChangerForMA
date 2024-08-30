namespace io.github.azukimochi;

internal static class AssetUtils
{
    public static T FromGUID<T>(string guid) where T : Object
        => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
}