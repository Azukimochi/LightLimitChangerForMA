namespace io.github.azukimochi;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class DisplayOptionAttribute : Attribute
{ 
    public string Label { get; set; }
    public string Description { get; set; }
}
