namespace io.github.azukimochi;

internal static class Utils
{
    public static float NormalizeInRange(float value, float min, float max)
        => (value - min) / (max - min);

    public static float ToFloat(this bool value) => value ? 1f : 0f;

    public static float ToFloat(this int value) => value;
}