using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace io.github.azukimochi
{
    [CustomPropertyDrawer(typeof(TargetShaders))]
    internal sealed class TargetShadersEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            var isEverything = property.FindPropertyRelative(nameof(TargetShaders.IsEverything));
            var targets = property.FindPropertyRelative(nameof(TargetShaders.Targets));
            string[] array = new string[targets.arraySize];
            for( int i = 0; i < array.Length; i++)
            {
                array[i] = targets.GetArrayElementAtIndex(i).stringValue;
            }
            var a = new TargetShaders() { IsEverything = isEverything.boolValue, Targets = array };
            var mask = a.ToBitMask();

            EditorGUI.BeginChangeCheck();

            mask = EditorGUI.MaskField(position, label, mask, ShaderInfo.RegisteredShaderInfoNames);

            if (EditorGUI.EndChangeCheck())
            {
                a.FromBitMask(mask);

                isEverything.boolValue = a.IsEverything;
                targets.ClearArray();

                if (a.Targets != null)
                for(int i = 0; i < a.Targets.Length; i++)
                {
                    targets.InsertArrayElementAtIndex(i);
                    targets.GetArrayElementAtIndex(i).stringValue = a.Targets[i];
                }
            }
            EditorGUI.EndProperty();
        }
    }
}
