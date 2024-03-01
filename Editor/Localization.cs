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
            { "label.category.general_settings", new []{ "General Settings", "一般設定"} },
            { "label.category.additional_settings", new []{ "Additional Settings", "追加設定"}},
            { "label.use_default", new[] { "Apply on Initial State", "初期状態で適用する" } },
            { "label.save_value", new[] { "Save Parameter State", "パラメータを保持する" } },
            { "label.override_min_max", new[] { "Overwrite Default Min/Max", "初期の上限と下限を上書きする" } },
            { "label.light_max", new[] { "Max Brightness", "明るさの上限" } },
            { "label.light_min", new[] { "Min Brightness", "明るさの下限" } },
            { "label.light_default", new[] { "Default Brightness", "明るさの初期値" } },
            { "category.select_option", new[] { "Options", "オプション" } },
            { "category.select_advanced", new[] { "Advanced Settings", "詳細設定" } },
            { "label.target_shader", new[] { "Target Shader", "対象シェーダー" } },
            { "info.shader_must_select" , new []{"Target shader must be selected", "対象シェーダーを選択してください"} },
            { "label.allow_color_tmp", new[] { "Enable Color Temperature Control", "色温度調整を有効にする" } },
            { "label.allow_saturation", new[] { "Enable Saturation Control", "彩度調整を有効にする" } },
            { "label.allow_monochrome", new []{ "Enable Monochrome Control", "ライトのモノクロ化調整を有効にする"}},
            { "label.allow_unlit", new[] { "Enable Unlit Control", "Unlit調整を有効にする" } },
            { "label.allow_reset", new[] { "Add Reset Button", "リセットボタンを追加する" } },
            { "label.allow_override_poiyomi", new[] { "Enable Override Poiyomi AnimatedFlag", "PoiyomiのAnimatedフラグを上書きする" } },
            { "label.allow_editor_only", new[] { "Exclude EditorOnly", "EditorOnlyを除外する" } },
            { "label.allow_gen_playmode", new[] { "Generate At Build/PlayMode", "ビルド・実行時に生成する" } },
            { "label.excludes", new[] { "Exclusion Settings", "除外設定" } },
            { "label.grouping_additional_controls", new[]{ "Group Additional Controls", "追加コントロールをグループ化する"} },
            { "label.separate_light_control", new[] { "Set Min/Max Brightness Individually", "明るさの上限と下限を別々に設定する" } },
            { "label.match_avatar", new[]{ "Match Avatar", "アバターに合わせる" } },
            { "label.separate_light_control_init_val", new []{ "Individual Initial Value Settings", "個別の初期値設定"}},
            { "label.light_min_default", new []{"Min Default Value", "明るさの下限の初期値"}},
            { "label.light_max_default", new []{ "Max Default Value", "明るさの上限の初期値"}},
            { "label.color_temp", new []{ "Color Temp", "色温度"}},
            { "label.saturation", new []{ "Saturation", "彩度"}},
            { "label.monochrome", new []{ "Monochrome", "モノクロ化"}},
            { "label.unlit", new []{ "Unlit", "Unlit"}},
            { "info.generate", new[] { "Generate", "生成" } },
            { "info.re_generate", new[] { "Regenerate", "再生成" } },
            { "info.process", new[] { "Processing", "生成中" } },
            { "info.complete", new[] { "Complete", "生成終了" } },
            { "info.error", new[] { "Error", "エラー" } },
            { "info.save", new[] { "Save", "保存" } },
            { "info.initial_val", new []{ "Default Values for Additional Settings", "追加設定の初期値" } },
            { "info.save_location", new[] { "Save Location", "アセットの保存場所" } },
            { "info.cancelled", new[] { "Cancelled", "キャンセルしました" } },
            { "tip.select_avatar", new[] { "Select the avatar to generate animations for", "アニメーションを生成するアバターをセットしてください" } },
            { "tip.use_default", new[] { "Use the light animation in the initial state", "初期状態でライトのアニメーションを使用します" } },
            { "tip.save_value", new[] { "Keep brightness changes in the avatar", "明るさの変更をアバターに保持したままにします" } },
            {
                "tip.override_min_max",
                new[]
                {
                    "Override default avatar brightness with the lower and upper limit parameters below",
                    "デフォルトのアバターの明るさを下限上限設定パラメータで上書きします"
                }
            },
            { "tip.light_max", new[] { "Brightness upper limit setting", "明るさの上限設定です" } },
            { "tip.light_min", new[] { "Brightness lower limit setting", "明るさの下限設定です" } },
            { "tip.light_default", new[] { "Initial brightness setting", "初期の明るさ設定" } },
            { "tip.target_shader", new[] { "Selects which shader(s) to control", "制御するシェーダーを選択できます" } },
            { "tip.allow_color_tmp", new[] { "Enables color temperature adjustment functionality", "色温度の調節機能を有効化することができます" } },
            { "tip.allow_saturation", new[] { "Enables saturation adjustment functionality", "彩度の調整機能を有効化することができます" } },
            { "tip.allow_monochrome", new []{ "Enables monochrome adjustment functionality", "ライトのモノクロ化の調整機能を有効化することができます"} },
            {
                "tip.allow_unlit",
                new[]
                {
                    "Enables Unlit adjustment functionality (Liltoon/Sunao Only)",
                    "Unlit の調整機能を有効化することができます(Liltoon/Sunao Only)"
                }
            },
            {
                "tip.allow_reset", new[] { "Adds a reset button to return parameters to selected values", "パラメータを設定値に戻すリセットボタンを追加します" }
            },
            {
                "tip.allow_override_poiyomi",
                new[] { "Automatically set animation flags at build time", "ビルド時に自動的にアニメーションフラグをセットします" }
            },
            {
                "tip.allow_editor_only",
                new[]
                {
                    "Exclude objects marked with EditorOnly tag from animations",
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
                @"Color temperature and/or saturation controls are enabled.

This feature non-destructively modifies materials and textures at runtime, which may cause unexpected bugs and increase texture memory consumption. 
If material colors appear abnormal, please disable these controls and file a bug report.",
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