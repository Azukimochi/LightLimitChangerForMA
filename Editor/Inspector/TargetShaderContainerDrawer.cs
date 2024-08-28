

using System.Numerics;

namespace io.github.azukimochi;

[CustomPropertyDrawer(typeof(TargetShaderContainer))]
internal sealed class TargetShaderContainerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        var container = (TargetShaderContainer)property.boxedValue;
        int mask;
        if (container.IsNothing)
        {
            mask = 0;
        }
        else if (container.IsEverything)
        {
            mask = -1;
        }
        else
        {
            mask = 0;
            foreach(var x in container.ShaderIds)
            {
                var v = ShaderManager.GetSupportedShaderIDs.IndexOf(x, StringComparison.Ordinal);
                if (v != -1)
                    mask |= 1 << v;
            }
        }

        EditorGUI.BeginChangeCheck();
        mask = EditorGUI.MaskField(position, label, mask, ShaderManager.GetSupportedShaderNames);
        if (EditorGUI.EndChangeCheck())
        {
            if (mask == 0)
            {
                container = TargetShaderContainer.Nothing;
            }
            else if (mask == -1)
            {
                container = TargetShaderContainer.Everything;
            }
            else
            {
                var array = new string[ShaderManager.GetSupportedShaderIDs.Length];
                for(int i = 0; i < array.Length; i++)
                {
                    if ((mask & (1 << i)) == 0)
                        continue;

                    array[i] = ShaderManager.GetSupportedShaderIDs[i];
                }
                container.ShaderIds = array;
            }
            property.boxedValue = container;
        }

        EditorGUI.EndProperty();
    }
}