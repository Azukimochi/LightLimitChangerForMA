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
            InitialValue = parameter.InitialValue switch
            {
                float x => x,
                int x => x,
                bool x => x ? 1 : 0,
                _ => default,
            },
            OverrideValue = parameter.OverrideValue switch
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
