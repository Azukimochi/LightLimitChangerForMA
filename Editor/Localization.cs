#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace io.github.azukimochi
{
    internal static class Localization
    {
        private const string PreferenceKey = "io.github.azukimochi.light-limit-changer.lang";
        private static int _SelectedLanguage = EditorPrefs.GetInt(PreferenceKey);
        private static readonly GUIContent[] _SupportedLanguages = new GUIContent[] { new GUIContent("日本語"), new GUIContent("English") };
        private static readonly GUIContent _Label = new GUIContent("Language");
        private static GUIContent _buffer = new GUIContent();

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
            { "Target Shader", "対象シェーダー" },
            { "Target Shader must be selected", "対象シェーダーを選択してください" },
            { "Allow Saturation Control", "彩度調整を有効にする" },
            { "Add Reset Button", "リセットボタンを追加する" },
            { "Exclude EditorOnly", "EditorOnlyを除外する" },
            { "Generate At Build/PlayMode", "ビルド・実行時に生成する" },
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

        public static GUIContent G(string text)
        {
            var buffer = _buffer;
            buffer.text = S(text);
            return buffer;
        }

        public static void ShowLocalizationUI()
        {
            var current = _SelectedLanguage;
            _SelectedLanguage = EditorGUILayout.Popup(_Label, current, _SupportedLanguages);
            if (current != _SelectedLanguage)
            {
                EditorPrefs.SetInt(PreferenceKey, _SelectedLanguage);
            }
        }
    }
}

#endif
