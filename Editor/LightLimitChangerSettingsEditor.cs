using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace io.github.azukimochi
{
    [CustomEditor(typeof(LightLimitChangerSettings))]
    internal sealed class LightLimitChangerSettingsEditor : Editor
    {
        private SerializedProperty FX;
        private SerializedProperty Parameters;

        private void OnEnable()
        {
            FX = serializedObject.FindProperty(nameof(LightLimitChangerSettings.FX));
            Parameters = serializedObject.FindProperty(nameof(LightLimitChangerSettings.Parameters));
        }

        public override void OnInspectorGUI()
        {
            using (new Utils.GroupScope(Localization.S("Parameter"), EditorGUIUtility.labelWidth))
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(FX);

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
