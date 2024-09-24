namespace io.github.azukimochi;

internal static class Utils
{
    public static float NormalizeInRange(float value, float min, float max)
        => (value - min) / (max - min);
}