namespace io.github.azukimochi;

internal static class ParameterExt
{
    public static Parameter<float> ToFloat<T>(this Parameter<T> parameter)
    {
        var result = new Parameter<float>()
        {
            Enable = parameter.Enable,
            Saved = parameter.Saved,
            Range = parameter.Range,
            Synced = parameter.Synced,
            Value = parameter.Value switch
            {
                float x => x,
                int x => x,
                bool x => x ? 1 : 0,
                _ => default,
            }
        };

        return result;
    }
}
