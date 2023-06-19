#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace io.github.azukimochi
{
    internal static class Localization
    {
        private static int _SelectedLanguage = 0;
        private static readonly GUIContent[] _SupportedLanguages = new GUIContent[] { new GUIContent("日本語"), new GUIContent("English") };
        private static readonly GUIContent _Label = new GUIContent("Language");

        private static Dictionary<string, string> _LocalizedText = new Dictionary<string, string>()
        {
            { "Select Avatar", "アバターを選択" },
            { "Avatar", "アバター" },
            { "Parameter", "パラメーター" },
            { "DefaultUse", "初期状態で適用する" },
            { "SaveValue", "パラメータを保持する" },
            { "MaxLight", "明るさの上限" },
            { "MinLight", "明るさの下限" },
            { "DefaultLight", "明るさの初期値" },
            { "Options", "オプション" },
            { "Allow Saturation Control", "彩度調整をオンにする" },
            { "Generate", "生成" },
            { "Regenerate", "再生成" },
            { "Processing", "生成中" },
            { "Complete", "生成終了" },
            { "Error", "エラー" },
            { "Save",  "保存" },
            { "Save Location",  "アセットの保存場所" },
            { "Cancelled", "キャンセルしました" },
        };

        public static string S(string text)
        {
            if (_SelectedLanguage == 0 && _LocalizedText.TryGetValue(text, out var res))
                return res;
            return text;
        }

        public static void ShowLocalizationUI()
        {
            _SelectedLanguage = EditorGUILayout.Popup(_Label, _SelectedLanguage, _SupportedLanguages);
        }
    }
}

#endif
