using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace io.github.azukimochi
{
    internal static class Localization
    {
        private const string PreferenceKey = "io.github.azukimochi.light-limit-changer.lang";
        private static int _SelectedLanguage = EditorPrefs.GetInt(PreferenceKey, 1);
        private static readonly GUIContent[] _SupportedLanguages = new GUIContent[] { new GUIContent("English (United States)"), new GUIContent("日本語 (日本)"), new GUIContent("中文 (繁體)") };
        private static readonly GUIContent _Label = new GUIContent("Language");

        private static Dictionary<string, string[]> _LocalizedText = new Dictionary<string, string[]>()
        {
            ///////////////////////////////////////////////////////
            // カテゴリ category
            { "category.select_avatar", new[] { "Select Avatar", "アバターを選択", "選擇 Avatar" } },
            { "category.select_parameter", new[] { "Parameter", "パラメーター", "參數" } },
            { "category.select_option", new[] { "Options", "オプション", "可選設定" } },
            { "category.select_advanced", new[] { "Advanced Settings", "詳細設定", "進階設定" } },
            { "category.save_settings", new [] { "Save Settings", "設定を保存", "儲存設定" } },

            ///////////////////////////////////////////////////////
            // ラベル label
            { "label.avatar", new[] { "Avatar", "アバター", "Avatar" } },
            { "label.category.general_settings", new []{ "General Settings", "一般設定", "一般設定" } },
            { "label.category.additional_settings", new []{ "Additional Settings", "追加設定", "追加設定"}},
            { "label.use_default", new[] { "Apply on Initial State", "初期状態で適用する", "預設開啟使用" } },
            { "label.save_value", new[] { "Save Parameter State", "パラメータを保持する", "保持狀態" } },
            { "label.override_min_max", new[] { "Overwrite Default Min/Max", "初期の上限と下限を上書きする", "覆蓋原本的上下限" } },
            { "label.light_max", new[] { "Max Brightness", "明るさの上限", "亮度上限" } },
            { "label.light_min", new[] { "Min Brightness", "明るさの下限", "亮度下限" } },
            { "label.light_default", new[] { "Default Brightness", "明るさの初期値", "預設亮度" } },
            { "label.changelog", new[] {"Changelog", "更新履歴", "更新日誌" } },
            { "label.target_shader", new[] { "Target Shader", "対象シェーダー", "目標 Shader" } },
            { "label.allow_color_tmp", new[] { "Enable Color Temperature Control", "色温度調整を有効にする", "啟用色溫控制" } },
            { "label.allow_saturation", new[] { "Enable Saturation Control", "彩度調整を有効にする", "啟用飽和度控制" } },
            { "label.allow_monochrome", new []{ "Enable Monochrome Control", "ライトのモノクロ化調整を有効にする", "啟用單色化控制"}},
            { "label.allow_unlit", new[] { "Enable Unlit Control", "Unlit調整を有効にする", "啟用 Unlit 控制" } },
            { "label.allow_reset", new[] { "Add Reset Button", "リセットボタンを追加する", "新增重置按鈕" } },
            { "label.allow_override_poiyomi", new[] { "Enable Override Poiyomi AnimatedFlag", "PoiyomiのAnimatedフラグを上書きする", "啟用覆蓋 Poiyomi AnimatedFlag" } },
            { "label.allow_editor_only", new[] { "Exclude EditorOnly", "EditorOnlyを除外する", "排除 EditorOnly" } },
            { "label.allow_gen_playmode", new[] { "Generate At Build/PlayMode", "ビルド・実行時に生成する", "在播放模式或建置時生成" } },
            { "label.excludes", new[] { "Exclusion Settings", "除外設定", "排除設定" } },
            { "label.grouping_additional_controls", new[]{ "Group Additional Controls", "追加コントロールをグループ化する", "追加選單組"} },
            { "label.separate_light_control", new[] { "Set Min/Max Brightness Individually", "明るさの上限と下限を別々に設定する", "分別設置亮度上下限" } },
            { "label.match_avatar", new[]{ "Match Avatar", "アバターに合わせる", "配合 Avatar" } },
            { "label.separate_light_control_init_val", new []{ "Individual Initial Value Settings", "個別の初期値設定", "個別預設值設定"}},
            { "label.light_min_default", new []{"Min Default Value", "明るさの下限の初期値", "亮度下限預設值"}},
            { "label.light_max_default", new []{ "Max Default Value", "明るさの上限の初期値", "亮度上限預設值"}},
            { "label.color_temp", new []{ "Color Temp", "色温度", "色溫"}},
            { "label.saturation", new []{ "Saturation", "彩度", "飽和度"}},
            { "label.monochrome", new []{ "Monochrome", "モノクロ化", "單色化"}},
            { "label.unlit", new []{ "Unlit", "Unlit", "Unlit"}},
            { "label.apply_settings_avatar", new []
            {
                "Set current settings as default and apply to other avatars",
                "現在の設定をデフォルトにして他のアバターに適用する",
                "將目前的設定設為預設值並套用至其他 Avatar",
            }},
            { "label.apply_settings_project", new []
            {
                "Apply to all Unity projects",
                "全てのUnityプロジェクトに適用する",
                "套用至全部 Unity 專案",
            }},
            { "label.document", new []
                {
                    "Documentation",
                    "説明書",
                    "說明書"
                }
            },

            ///////////////////////////////////////////////////////
            // 情報　info
            { "info.shader_must_select" , new []{"Target shader must be selected", "対象シェーダーを選択してください", "必須選擇目標 Shader"} },
            { "info.generate", new[] { "Generate", "生成", "生成" } },
            { "info.re_generate", new[] { "Regenerate", "再生成", "再生成" } },
            { "info.process", new[] { "Processing", "生成中", "生成中" } },
            { "info.complete", new[] { "Complete", "生成終了", "完成" } },
            { "info.error", new[] { "Error", "エラー", "錯誤" } },
            { "info.save", new[] { "Save", "保存", "儲存" } },
            { "info.initial_val", new []{ "Default Values for Additional Settings", "追加設定の初期値", "追加設定的預設值" } },
            { "info.save_location", new[] { "Save Location", "アセットの保存場所", "資源儲存位置" } },
            { "info.cancelled", new[] { "Cancelled", "キャンセルしました", "已取消" } },
            
            ///////////////////////////////////////////////////////
            // ヒント tip
            { "tip.select_avatar", new[] { "Select the avatar to generate animations for", "アニメーションを生成するアバターをセットしてください", "選擇要為其生成動畫的 Avatar" } },
            { "tip.use_default", new[] { "Use the light animation in the initial state", "初期状態でライトのアニメーションを使用します", "在預設狀態下使用亮度動畫" } },
            { "tip.save_value", new[] { "Keep brightness changes in the avatar", "明るさの変更をアバターに保持したままにします", "保持 Avatar 的亮度變化" } },
            {
                "tip.override_min_max", new[]
                {
                    "Override default avatar brightness with the lower and upper limit parameters below",
                    "デフォルトのアバターの明るさを下限上限設定パラメータで上書きします",
                    "使用此組件的上下限設定覆蓋 Avatar 原本的亮度上下限"
                }
            },
            { "tip.light_max", new[] { "Brightness upper limit setting", "明るさの上限設定です", "亮度的上限設定" } },
            { "tip.light_min", new[] { "Brightness lower limit setting", "明るさの下限設定です", "亮度的下限設定" } },
            { "tip.light_default", new[] { "Initial brightness setting", "初期の明るさ設定", "預設的亮度設定" } },
            { "tip.target_shader", new[] { "Selects which shader(s) to control", "制御するシェーダーを選択できます", "選擇要控制哪個或哪些著色器" } },
            { "tip.allow_color_tmp", new[] { "Enables color temperature adjustment functionality", "色温度の調節機能を有効化することができます", "啟用色溫調整功能"  } },
            { "tip.allow_saturation", new[] { "Enables saturation adjustment functionality", "彩度の調整機能を有効化することができます", "啟用飽和度調整功能" } },
            { "tip.allow_monochrome", new []{ "Enables monochrome adjustment functionality", "ライトのモノクロ化の調整機能を有効化することができます", "啟用單色化調整功能"} },
            {
                "tip.allow_unlit", new[]
                {
                    "Enables Unlit adjustment functionality (Liltoon/Sunao Only)",
                    "Unlit の調整機能を有効化することができます(Liltoon/Sunao Only)",
                    "啟用 Unlit 調整功能（僅 Liltoon/Sunao）"
                }
            },
            {
                "tip.allow_reset", new[] { "Adds a reset button to return parameters to selected values", "パラメータを設定値に戻すリセットボタンを追加します", "新增一個重置按鈕以回到預設值" }
            },
            {
                "tip.allow_override_poiyomi",
                new[] { "Automatically set animation flags at build time", "ビルド時に自動的にアニメーションフラグをセットします", "在建置時自動設定動畫標誌" }
            },
            {
                "tip.allow_editor_only", new[]
                {
                    "Exclude objects marked with EditorOnly tag from animations",
                    "EditorOnlyタグに設定されているオブジェクトをアニメーションから除外します",
                    "從動畫中排除標記為 EditorOnly 的對象"
                }
            },
            {
                "tip.allow_gen_playmode",
                new[] { "Automatically generate animations at build/play mode", "ビルド・実行時にアニメーションを自動生成します", "在播放模式或建置時自動生成動畫" }
            },
            
            ///////////////////////////////////////////////////////
            // ウインドウ　window
            {
                "window.info.deprecated", new[]
                {
                    "The settings window has been deprecated. See below for new ways of doing things.",
                    "設定ウィンドウは非推奨になりました。新しいやり方については以下をご覧ください。",
                    "這個設定介面已被棄用，請參閱下文以了解新方法。"
                }
            },
            {
                "Window.info.global_settings.changed", new []
                {
                    "Light Limit Changer global settings changed",
                    "Light Limit Changerのグローバル設定が変更されました",
                    "Light Limit Changer 全域設定已變更。"
                }
            },
            {
                "Window.info.global_settings.update_available", new []
                {
                    "Update settings",
                    "設定を更新",
                    "更新設定"
                }
            },
            {
                "Window.info.global_settings.title", new []
                {
                    "Loading Global Settings",
                    "グローバル設定の読み込み",
                    "加載全域設定"
                }
            },
            {
                "Window.info.global_settings.message", new []
                {
                    @"Global settings are available. Would you like to load them?
Pressing Cancel will discard the global settings and keep the current settings. ",
                    @"グローバル設定が利用可能です。読み込みますか？
キャンセルを押すとグローバル設定を破棄して現在の設定を保持します。",
                    @"全域設定為可用。要載入它嗎？
按下取消將放棄全域設定並保留目前設定。",
                }
            },
            {
                "Window.info.gloabl_settings.save", new []
                {
                    "Save as Global Settings",
                    "グローバル設定として保存",
                    "儲存為全域設定"
                }
            },
            {
                "Window.info.global_settings.save_message", new []
                {
                    @"Using this setting will apply the setting to all other Unity projects.
Settings will be loaded in another project only after Unity is restarted.",
                    @"この設定を使用すると、他のすべてのUnityプロジェクトへ設定が適用されます。
別のプロジェクトで設定が読み込まれるのはUnity再起動後です。",
                    @"使用此設定將設置套用於其他所有的 Unity 專案。
在另一個專案中，設定僅在重啟 Unity 後才會被加載。",
                }
            },
            {
                "Window.info.choice.apply_save", new []
                {
                    "Apply as Global Settings",
                    "グローバル設定として適用",
                    "套用為全域設定"
                }
            },
            {
                "Window.info.choice.apply_load", new []
                {
                    "Load Global Settings",
                    "グローバル設定を読み込む",
                    "載入全域設定"
                }
            },
            {
                "Window.info.error.already_setup", new []
                {
                    //[0] = Avatar Name
                    "Light Limit Changer has already been installed in the avatar \"{0}\"",
                    "アバター「{0}」にはすでにLight Limit Changerが導入されています",
                    "Avatar「{0}」已安裝 Light Limit Changer"
                }
            },
            {
                "Window.info.cancel", new []
                {
                    "Cancel",
                    "キャンセル",
                    "取消"
                }
            },
            {
                "window.info.ok", new []
                {
                    "OK",
                    "OK",
                    "OK"
                }
            },
            ///////////////////////////////////////////////////////
            // Links
            {
                "link.document.changelog", new []
                {
                    "Light Limit Changer OfficialSite | Changelog",
                    "Light Limit Changer OfficialSite | 更新履歴",
                    "Light Limit Changer OfficialSite | 更新日誌"
                }
            },
            {
                "link.document.recommend", new []
                {
                    "Light Limit Changer OfficialSite | Recommend Settings",
                    "Light Limit Changer OfficialSite | おすすめ設定",
                    "Light Limit Changer OfficialSite | 推薦設定"
                }
            },
            {
                "link.document.description", new []
                {
                    "Light Limit Changer OfficialSite | Description",
                    "Light Limit Changer OfficialSite | 設定概要",
                    "Light Limit Changer OfficialSite | 概述"
                }
            },
            
            ///////////////////////////////////////////////////////
            // NDMF
            {
                "NDMF.info.useColorTemporSaturation", new[]
                {
                @"Color temperature and/or saturation controls are enabled.

This feature non-destructively modifies materials and textures at runtime, which may cause unexpected bugs and increase texture memory consumption. 
If material colors appear abnormal, please disable these controls and file a bug report.",
                @"色温度または彩度の変更が有効になっています。

この機能は実行時に非破壊でマテリアル・テクスチャを変更するため、潜在的なバグやテクスチャメモリが増加する可能性があります。
マテリアルの色がおかしくなった場合にはこの機能を使用せず、作者に報告をお願いします。",
                @"已啟用色溫或飽和度控制功能。

此功能在運行時會以非破壞性方式修改材質和貼圖，這可能會導致潛在的錯誤和增加貼圖記憶體消耗。
如果材質顏色顯示異常，請不要使用這些功能並向作者報告。"
                }
            },
            {
                "NDMF.info.poiyomi_old_version", new[]
                {
                    @"Poiyomi Shader version 7.3 is detected.
Please update to the latest version of Poiyomi Shader.",
                    @"Poiyomi Shader のバージョン 7.3 が検出されました。
Poiyomi Shaderの最新版へのアップデートをお願いします",
                    @"檢測到 Poiyomi Shader 7.3 版本。
請使用最新版的 Poiyomi Shader。"
                }
            },
            {
                "NDMF.info.non_generated", new []
                {
                    @"The menu was not generated because there was no animation target.
Please check your settings if this is not what you intended.",
                    @"アニメーションの生成対象が存在しなかったため、メニューを生成しませんでした。
これが意図したものでない場合には、設定を確認してください。",
                    @"選單未生成，因為沒有要動畫的目標。
如果你不是有意的，請檢查你的設定。"
                }
            },
            
            /////////////////////////////////////////////////
            // ExpressionMenu
            {
                "ExpressionMenu.light", new []
                {
                    "Light",
                    "明るさ",
                    "亮度"
                }
            },
            {
                "ExpressionMenu.light_min", new []
                {
                    "Min Light",
                    "明るさの下限",
                    "亮度下限"
                }
            },
            {
                "ExpressionMenu.light_max", new []
                {
                    "Max Light",
                    "明るさの上限",
                    "亮度上限"
                }
            },
            {
                "ExpressionMenu.color_temp", new []
                {
                    "Color Temp",
                    "色温度",
                    "色溫"
                }
            },
            {
                "ExpressionMenu.saturation", new []
                {
                    "Saturation",
                    "彩度",
                    "飽和度"
                }
            },
            {
                "ExpressionMenu.unlit", new []
                {
                    "Unlit",
                    "Unlit",
                    "Unlit"
                }
            },
            {
                "ExpressionMenu.monochrome", new []
                {
                    "Monochrome",
                    "モノクロ化",
                    "單色化"
                }
            },
            {
                "ExpressionMenu.Enable", new []
                {
                    "Enable",
                    "有効",
                    "啟用"
                }
            },
            {
                "ExpressionMenu.reset", new []
                {
                    "Reset",
                    "リセット",
                    "重置"
                }
            },
            {
                "ExpressionMenu.Control", new []
                {
                    "Control",
                    "コントロール",
                    "控制"
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
