using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace io.github.azukimochi
{
    internal static class Localization
    {
        private const string PreferenceKey = "io.github.azukimochi.light-limit-changer.lang";
        private static int _SelectedLanguage = EditorPrefs.GetInt(PreferenceKey, 1);
        private static readonly GUIContent[] _SupportedLanguages = new GUIContent[] { new GUIContent("English"), new GUIContent("日本語") };
        private static readonly GUIContent _Label = new GUIContent("Language");

        private static Dictionary<string, string[]> _LocalizedText = new Dictionary<string, string[]>()
        {
            { "category.select_avatar", new[] { "Select Avatar", "アバターを選択" } },
            { "label.avatar", new[] { "Avatar", "アバター" } },
            { "category.select_parameter", new[] { "Parameter", "パラメーター" } },
            { "label.category.general_settings", new[] { "General settings", "一般設定" } },
            { "label.category.additional_settings", new[] { "Additional settings", "追加設定" } },
            { "label.use_default", new[] { "DefaultUse", "初期状態で適用する" } },
            { "label.save_value", new[] { "SaveValue", "パラメータを保持する" } },
            { "label.override_min_max", new[] { "Overwrite Default Min/Max", "初期の上限と下限を上書きする" } },
            { "label.light_max", new[] { "MaxLight", "明るさの上限" } },
            { "label.light_min", new[] { "MinLight", "明るさの下限" } },
            { "label.light_default", new[] { "DefaultLight", "明るさの初期値" } },
            { "category.select_option", new[] { "Options", "オプション" } },
            { "category.select_advanced", new[] { "Advanced Setting", "詳細設定" } },
            { "label.target_shader", new[] { "Target Shader", "対象シェーダー" } },
            { "info.shader_must_select", new[] { "Target Shader must be selected", "対象シェーダーを選択してください" } },
            { "label.allow_color_tmp", new[] { "Allow Color Temperature Ctrl", "色温度調整を有効にする" } },
            { "label.allow_saturation", new[] { "Allow Saturation Control", "彩度調整を有効にする" } },
            { "label.allow_monochrome", new[] { "Allow Monochrome Control", "ライトのモノクロ化調整を有効にする" } },
            { "label.allow_unlit", new[] { "Allow Unlit Control", "Unlit調整を有効にする" } },
            { "label.allow_reset", new[] { "Add Reset Button", "リセットボタンを追加する" } },
            {
                "label.allow_override_poiyomi",
                new[] { "Allow Override Poiyomi AnimatedFlag", "PoiyomiのAnimatedフラグを上書きする" }
            },
            { "label.allow_editor_only", new[] { "Exclude EditorOnly", "EditorOnlyを除外する" } },
            { "label.allow_gen_playmode", new[] { "Generate At Build/PlayMode", "ビルド・実行時に生成する" } },
            { "label.excludes", new[] { "Exclusion Settings", "除外設定" } },
            { "label.grouping_additional_controls", new[] { "Grouping additional controls", "追加コントロールをグループ化する" } },
            {
                "label.separate_light_control",
                new[] { "Setting the upper and lower brightness limits separately", "明るさの上限と下限を別々に設定する" }
            },
            { "label.match_avatar", new[] { "Match Avatar", "アバターに合わせる" } },
            { "label.separate_light_control_init_val", new[] { "Individual initial value settings", "個別の初期値設定" } },
            { "label.light_min_default", new[] { "Min Default Value", "明るさの下限の初期値" } },
            { "label.light_max_default", new[] { "Max Default value", "明るさの上限の初期値" } },
            { "label.color_temp", new[] { "Color temp", "色温度" } },
            { "label.saturation", new[] { "Saturation", "彩度" } },
            { "label.monochrome", new[] { "Monochrome", "モノクロ化" } },
            { "label.unlit", new[] { "Unlit", "Unlit" } },
            { "info.generate", new[] { "Generate", "生成" } },
            { "info.re_generate", new[] { "Regenerate", "再生成" } },
            { "info.process", new[] { "Processing", "生成中" } },
            { "info.complete", new[] { "Complete", "生成終了" } },
            { "info.error", new[] { "Error", "エラー" } },
            { "info.save", new[] { "Save", "保存" } },
            { "info.initial_val", new[] { "Default values for additional settings", "追加設定の初期値" } },
            { "info.save_location", new[] { "Save Location", "アセットの保存場所" } },
            { "info.cancelled", new[] { "Cancelled", "キャンセルしました" } },
            { "tip.select_avatar", new[] { "Set the avatar to generate animation", "アニメーションを生成するアバターをセットしてください" } },
            { "tip.use_default", new[] { "Use the light animation in the initial state", "初期状態でライトのアニメーションを使用します" } },
            { "tip.save_value", new[] { "Keep brightness changes in the avatar", "明るさの変更をアバターに保持したままにします" } },
            {
                "tip.override_min_max",
                new[]
                {
                    "Override the default avatar brightness with the lower and upper limit parameters below",
                    "デフォルトのアバターの明るさを下限上限設定パラメータで上書きします"
                }
            },
            { "tip.light_max", new[] { "Brightness upper limit setting", "明るさの上限設定です" } },
            { "tip.light_min", new[] { "Brightness lower limit setting", "明るさの下限設定です" } },
            { "tip.light_default", new[] { "Initial brightness setting", "初期の明るさ設定" } },
            { "tip.target_shader", new[] { "You can choose which shader to control", "制御するシェーダーを選択できます" } },
            {
                "tip.allow_color_tmp",
                new[] { "You can enable the Color Temperature adjustment function", "色温度の調節機能を有効化することができます" }
            },
            {
                "tip.allow_saturation",
                new[] { "You can enable the saturation adjustment function", "彩度の調整機能を有効化することができます" }
            },
            {
                "tip.allow_monochrome",
                new[] { "You can enable the monochrome adjustment function", "ライトのモノクロ化の調整機能を有効化することができます" }
            },
            {
                "tip.allow_unlit",
                new[]
                {
                    "You can enable the Unlit adjustment function (Liltoon/Sunao Only)",
                    "Unlit の調整機能を有効化することができます(Liltoon/Sunao Only)"
                }
            },
            {
                "tip.allow_reset",
                new[] { "Add a reset button to return the parameter to the set value", "パラメータを設定値に戻すリセットボタンを追加します" }
            },
            {
                "tip.allow_override_poiyomi",
                new[] { "Automatically set animation flags at build time", "ビルド時に自動的にアニメーションフラグをセットします" }
            },
            {
                "tip.allow_editor_only",
                new[]
                {
                    "Exclude objects marked with EditorOnly tag from animation",
                    "EditorOnlyタグに設定されているオブジェクトをアニメーションから除外します"
                }
            },
            {
                "tip.allow_gen_playmode",
                new[] { "Automatically generate animations at build/play mode", "ビルド・実行時にアニメーションを自動生成します" }
            },
            {
                "window.info.deprecated",
                new[]
                {
                    "The settings window has been deprecated. See below for new ways of doing things.",
                    "設定ウィンドウは非推奨になりました。新しいやり方については以下をご覧ください。"
                }
            },

            {
                "NDMF.info.useColorTemporSaturation", new[]
                {
                    @"Color temperature or saturation changes are enabled.

This feature non-destructively modifies materials and textures at runtime, which may result in potential bugs and increased texture memory. 
If the color of the material becomes strange, please do not use this function and report it to the author.",
                    @"色温度または彩度の変更が有効になっています。

この機能は実行時に非破壊でマテリアル・テクスチャを変更するため、潜在的なバグやテクスチャメモリが増加する可能性があります。
マテリアルの色がおかしくなった場合にはこの機能を使用せず、作者に報告をお願いします。"
                }
            },
            {
                "NDMF.info.poiyomi_old_version", new[]
                {
                    @"Poiyomi Shader version 7.3 is detected.
Please update to the latest version of Poiyomi Shader.",
                    @"Poiyomi Shader のバージョン 7.3 が検出されました。
Poiyomi Shaderの最新版へのアップデートをお願いします"
                }
            }
        };

        public static string S(string text, int? language = null)
        {
            if (text != null)
            {
                if (_LocalizedText.TryGetValue(text, out var res))
                    return res[language ?? _SelectedLanguage];
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