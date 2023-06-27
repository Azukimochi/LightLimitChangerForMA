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

        private static Dictionary<string, string> _LocalizedText = new Dictionary<string, string>()
        {
            { "Select Avatar", "アバターを選択" },
            { "Avatar", "アバター" },
            { "Parameter", "パラメーター" },
            { "DefaultUse", "初期状態で適用する" },
            { "SaveValue", "パラメータを保持する" },
            { "Overwrite Default Min/Max", "初期の上限と下限を上書きする" },
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
            { "Set the avatar to generate animation", "アニメーションを生成するアバターをセットしてください"},
            { "Use the light animation in the initial state", "初期状態でライトのアニメーションを使用します"},
            { "Keep brightness changes in the avatar", "明るさの変更をアバターに保持したままにします"},
            { "Override the default avatar brightness with the lower and upper limit parameters below", "デフォルトのアバターの明るさを以下の下限上限設定パラメータで上書きします"},
            { "Brightness upper limit setting [0-10]", "明るさの上限設定です [0-10]" },
            { "Brightness lower limit setting [0-10]", "明るさの下限設定です [0-10]" },
            { "Initial brightness setting [0-1]", "初期の明るさ設定 [0-1]" },
            { "You can choose which shader to control", "制御するシェーダーを選択できます" },
            { "You can enable the saturation adjustment function", "彩度の調整機能を有効化することができます"},
            { "Add a reset button to return the parameter to the set value", "パラメータを設定値に戻すリセットボタンを追加します" },
            { "Exclude objects marked with EditorOnly tag from animation", "EditorOnlyタグに設定されているオブジェクトをアニメーションから除外します" },
            { "Automatically generate animations at build/play mode", "ビルド・実行時にアニメーションを自動生成します" }
        };

        public static string S(string text)
        {
            if(text != null)
            {
                if (_SelectedLanguage == 0 && _LocalizedText.TryGetValue(text, out var res))
                    return res;
            }
            return text;
        }

        public static GUIContent G(string text, string textTip = null) => Utils.Label(S(text), S(textTip));

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
