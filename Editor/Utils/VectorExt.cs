namespace io.github.azukimochi;

internal static class VectorExt
{
    public static void Deconstruct(this Vector2? vector2, out float x, out float y)
    {
        if (vector2 is not { } v)
        {
            (x, y) = (0, 0);
        }
        else
        {
            (x, y) = v;
        }
    }

    public static void Deconstruct(this Vector2 vector2, out float x, out float y)
        => (x, y) = (vector2.x, vector2.y);
}
