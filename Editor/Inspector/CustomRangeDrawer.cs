namespace io.github.azukimochi;

[CustomPropertyDrawer(typeof(RangeAttribute))]
internal sealed class CustomRangeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Generic)
        {
            // たぶんParameter<T>だと思うので
            return ParameterDrawer.GetPropertyHeight(property);
        }
        else
        {
            // TODO: floatとかintに対応しておく
        }
        return base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = attribute as RangeAttribute;
        if (property.propertyType == SerializedPropertyType.Generic)
        {
            var range = new Vector2(attr.Min, attr.Max);
            ParameterDrawer.Draw(position, property, label, range);
        }
        else
        {
            // 同上
        }
    }
}