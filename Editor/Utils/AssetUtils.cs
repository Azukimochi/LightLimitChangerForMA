namespace io.github.azukimochi;

internal static class AssetUtils
{
    public static T FromGUID<T>(string guid) where T : Object
    { 
        var path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path))
            return null;
        return AssetDatabase.LoadAssetAtPath<T>(path); 
    }
}