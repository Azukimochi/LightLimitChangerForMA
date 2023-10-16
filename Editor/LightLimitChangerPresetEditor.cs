using System;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace io.github.azukimochi
{
    [CustomEditor(typeof(LightLimitChangerPreset))]
    internal sealed class LightLimitChangerPresetEditor : Editor
    {
        private SerializedProperty _enable;

        private SerializedProperty _light;
        private SerializedProperty _lightMin;
        private SerializedProperty _lightMax;
        private SerializedProperty _saturation;
        private SerializedProperty _colorTemperature;
        private SerializedProperty _unlit;


        private void OnEnable()
        {
            _enable = new SerializedObject((serializedObject.targetObject as Component).gameObject).FindProperty("m_IsActive");
            _light = serializedObject.FindProperty(nameof(LightLimitChangerPreset.Light));
            _lightMin = serializedObject.FindProperty(nameof(LightLimitChangerPreset.LightMin));
            _lightMax = serializedObject.FindProperty(nameof(LightLimitChangerPreset.LightMax));
            _saturation = serializedObject.FindProperty(nameof(LightLimitChangerPreset.Saturation));
            _colorTemperature = serializedObject.FindProperty(nameof(LightLimitChangerPreset.ColorTemperature));
            _unlit = serializedObject.FindProperty(nameof(LightLimitChangerPreset.Unlit));
        }

        public override void OnInspectorGUI()
        {
            var parent = (target as LightLimitChangerPreset).GetParent();
            if (parent == null)
            {
                // TODO: プリセットがLLCの配下にないので警告を出す
                return;
            }
            serializedObject.Update();
            _enable.serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_enable);
            if (EditorGUI.EndChangeCheck())
            {
                _enable.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.BeginDisabledGroup(parent.Parameters.IsSeparateLightControl);
            DrawProperty(_light);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!parent.Parameters.IsSeparateLightControl);
            DrawProperty(_lightMin);
            DrawProperty(_lightMax);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Separator();

            var control = parent.Parameters.GetControlTypeFlags();

            EditorGUI.BeginDisabledGroup(!control.HasFlag(LightLimitControlType.ColorTemperature));
            DrawProperty(_colorTemperature);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!control.HasFlag(LightLimitControlType.Saturation));
            DrawProperty(_saturation);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!control.HasFlag(LightLimitControlType.Unlit));
            DrawProperty(_unlit);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawProperty(SerializedProperty property, float min = 0, float max = 1) => DrawProperty(property, new GUIContent(property.displayName), min, max);

        private static void DrawProperty(SerializedProperty property, GUIContent label, float min = 0, float max = 1)
        {
            var rect = EditorGUILayout.GetControlRect(true);
            label = EditorGUI.BeginProperty(rect, label, property);

            var enable = property.FindPropertyRelative(nameof(LightLimitChangerPreset.Parameter.Enable));
            var value = property.FindPropertyRelative(nameof(LightLimitChangerPreset.Parameter.Value));

            var labelRect = rect;
            var checkBoxRect = rect;
            var sliderRect = rect;

            labelRect.width = EditorGUIUtility.labelWidth + 1.5f; // nazo margin...
            checkBoxRect.width = checkBoxRect.height;
            checkBoxRect.x += labelRect.width;

            sliderRect.width -= labelRect.width + checkBoxRect.width + 8;
            sliderRect.x += labelRect.width + checkBoxRect.width + 8;

            EditorGUI.LabelField(labelRect, label);

            EditorGUI.PropertyField(checkBoxRect, enable, GUIContent.none);
            EditorGUI.BeginDisabledGroup(!enable.boolValue);
            value.floatValue = EditorGUI.Slider(sliderRect, value.floatValue, min, max);
            EditorGUI.EndDisabledGroup();


            EditorGUI.EndProperty();
        }
    }
}
