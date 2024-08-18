using Target = io.github.azukimochi.LightLimitChangerComponent;

namespace io.github.azukimochi;

[CustomEditor(typeof(Target))]
internal sealed class LightLimitChangerComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
