namespace io.github.azukimochi;

internal static class MaterialExt
{
    public static T Get<T>(this Material material, string propertyName, T defaultValue = default)
    {
        if (!material.HasProperty(propertyName))
            return defaultValue;

        if (typeof(T) == typeof(int))
            return (T)(object)material.GetInt(propertyName);

        if (typeof(T) == typeof(float))
            return (T)(object)material.GetFloat(propertyName);

        if (typeof(T) == typeof(Color))
            return (T)(object)material.GetColor(propertyName);

        if (typeof(T) == typeof(Vector4))
            return (T)(object)material.GetVector(propertyName);

        if (typeof(Texture).IsAssignableFrom(typeof(T)))
        {
            var t = material.GetTexture(propertyName);
            if (t is T)
                return (T)(object)t;
        }

        return default;
    }
}