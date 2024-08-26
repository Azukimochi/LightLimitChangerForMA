using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace io.github.azukimochi
{
    internal static class Localization
    {
        private const string PreferenceKey = "io.github.azukimochi.light-limit-changer.lang";
        private static int _SelectedLanguage = EditorPrefs.GetInt(PreferenceKey, 1);
        private static readonly GUIContent[] _SupportedLanguages = new GUIContent[]
        {
            new GUIContent("English (United States)"),
            new GUIContent("日本語 (日本)"),
            new GUIContent("中文（繁體）"),
            new GUIContent("中文（简体）"),
            new GUIContent("한국어 (대한민국)")
        };
        private static readonly GUIContent _Label = new GUIContent("Language");

        private static Dictionary<string, string[]> _LocalizedText = new Dictionary<string, string[]>()
        {
            ///////////////////////////////////////////////////////
            // カテゴリ category
            {
                "category.select_avatar", new []
                {
                    "Select Avatar",
                    "アバターを選択",
                    "選擇 Avatar",
                    "选择 Avatar",
                    "아바타 선택"
                }
            },
            {
                "category.select_parameter", new []
                {
                    "Parameter",
                    "パラメーター",
                    "參數",
                    "参数",
                    "파라미터"
                }
            },
            {
                "category.select_option", new []
                {
                    "Options",
                    "オプション",
                    "可選設定",
                    "选项",
                    "옵션"
                }
            },
            {
                "category.select_advanced", new []
                {
                    "Advanced Settings",
                    "詳細設定",
                    "進階設定",
                    "高级设置",
                    "상세 설정"
                }
            },
            {
                "category.save_settings", new []
                {
                    "Save Settings",
                    "設定を保存",
                    "儲存設定",
                    "保存设置",
                    "설정 저장"
                }
            },

            ///////////////////////////////////////////////////////
            // ラベル label
            {
                "label.avatar", new []
                {
                    "Avatar",
                    "アバター",
                    "Avatar",
                    "Avatar",
                    "아바타"
                }
            },
            {
                "label.category.general_settings", new []
                {
                    "General Settings",
                    "一般設定",
                    "一般設定",
                    "常规设置",
                    "일반 설정"
                }
            },
            {
                "label.category.additional_settings", new []
                {
                    "Additional Settings",
                    "追加設定",
                    "追加設定",
                    "附加设置",
                    "추가 설정"
                }
            },
            {
                "label.use_default", new []
                {
                    "Apply on Initial State",
                    "初期状態で適用する",
                    "預設開啟使用",
                    "初始状态为启用",
                    "초기 상태에 적용"
                }
            },
            {
                "label.save_value", new []
                {
                    "Save Parameter State",
                    "パラメータを保持する",
                    "保持狀態",
                    "参数设为保存",
                    "파라미터 상태 저장"
                }
            },
            {
                "label.override_min_max", new []
                {
                    "Overwrite Default Min/Max",
                    "初期の上限と下限を上書きする",
                    "覆蓋原本的上下限",
                    "覆盖原本的亮度上下限",
                    "머티리얼의 최소/최대 밝기 덮어쓰기"
                }
            },
            {
                "label.light_max", new []
                {
                    "Max Brightness",
                    "明るさの上限",
                    "亮度上限",
                    "亮度上限",
                    "최대 밝기"
                }
            },
            {
                "label.light_min", new []
                {
                    "Min Brightness",
                    "明るさの下限",
                    "亮度下限",
                    "亮度下限",
                    "최소 밝기"
                }
            },
            {
                "label.light_default", new []
                {
                    "Default Brightness",
                    "明るさの初期値",
                    "預設亮度",
                    "预设亮度",
                    "밝기 초기값"
                }
            },
            {
                "label.changelog", new []
                {
                    "Changelog",
                    "更新履歴",
                    "更新日誌",
                    "更新日志",
                    "변경 사항"
                }
            },
            {
                "label.target_shader", new []
                {
                    "Target Shader",
                    "対象シェーダー",
                    "目標 Shader",
                    "目标 Shader",
                    "대상 셰이더"
                }
            },
            {
                "label.allow_color_tmp", new []
                {
                    "Enable Color Temperature Control",
                    "色温度調整を有効にする",
                    "啟用色溫控制",
                    "启用色温控制",
                    "색온도 조정 활성화"
                }
            },
            {
                "label.allow_saturation", new []
                {
                    "Enable Saturation Control",
                    "彩度調整を有効にする",
                    "啟用飽和度控制",
                    "启用饱和度控制",
                    "채도 조정 활성화"
                }
            },
            {
                "label.allow_monochrome", new []
                {
                    "Enable Monochrome Control",
                    "ライトのモノクロ化調整を有効にする",
                    "啟用單色化控制",
                    "启用单色化控制",
                    "흑백화(Monochrome) 조정 활성화"
                }
            },
            {
                "label.allow_unlit", new []
                {
                    "Enable Unlit Control",
                    "Unlit調整を有効にする",
                    "啟用 Unlit 控制",
                    "启用 Unlit 控制",
                    "Unlit 조정 활성화"
                }
            },
            {
                "label.allow_emission", new []
                {
                    "Enable Emission Control",
                    "エミッションの調整を有効にする",
                    "啟用自發光控制",
                    "启用自发光控制",
                    "발광(Emission) 조정 활성화"
                }
            },
            {
                "label.allow_reset", new []
                {
                    "Add Reset Button",
                    "リセットボタンを追加する",
                    "新增重置按鈕",
                    "添加重置按钮",
                    "초기화 버튼 추가"
                }
            },
            {
                "label.allow_override_poiyomi", new []
                {
                    "Enable Override Poiyomi AnimatedFlag",
                    "PoiyomiのAnimatedフラグを上書きする",
                    "啟用覆蓋 Poiyomi AnimatedFlag",
                    "启用 Poiyomi AnimatedFlag 覆盖",
                    "Poiyomi의 Animated 플래그 덮어쓰기"
                }
            },
            {
                "label.allow_editor_only", new []
                {
                    "Exclude EditorOnly",
                    "EditorOnlyを除外する",
                    "排除 EditorOnly",
                    "排除 EditorOnly",
                    "EditorOnly 제외"
                }
            },
            {
                "label.allow_gen_playmode", new []
                {
                    "Generate At Build/PlayMode",
                    "ビルド・実行時に生成する",
                    "在播放模式或建置時生成",
                    "在构建或播放模式时生成",
                    "빌드 및 PlayMode에서 실행"
                }
            },
            {
                "label.excludes", new []
                {
                    "Exclusion Settings",
                    "除外設定",
                    "排除設定",
                    "排除设置",
                    "제외 설정"
                }
            },
            {
                "label.grouping_additional_controls", new []
                {
                    "Group Additional Controls",
                    "追加コントロールをグループ化する",
                    "追加選單組",
                    "添加菜单组",
                    "추가 제어 옵션 그룹화"
                }
            },
            {
                "label.separate_light_control", new []
                {
                    "Set Min/Max Brightness Individually",
                    "明るさの上限と下限を別々に設定する",
                    "分別設置亮度上下限",
                    "分别设置亮度上下限",
                    "최소/최대 밝기 개별 설정"
                }
            },
            {
                "label.separate_monochrome_control", new []
                 {
                    "Set Monochrome Individually",
                    "モノクロを別々に設定する",
                    "Set Monochrome Individually",
                    "Set Monochrome Individually",
                    "흑백화(Monochrome) 개별 설정"
                 }
            },
            {
                "label.match_avatar", new []
                {
                    "Match Avatar",
                    "アバターに合わせる",
                    "配合 Avatar",
                    "匹配 Avatar",
                    "아바타와 일치"
                }
            },
            {
                "label.separate_light_control_init_val", new []
                {
                    "Individual Initial Value Settings",
                    "個別の初期値設定",
                    "個別預設值設定",
                    "分别设置预设值",
                    "초기값 개별 설정"
                }
            },
            {
                "label.light_min_default", new []
                {
                    "Min Default Value",
                    "明るさの下限の初期値",
                    "亮度下限預設值",
                    "亮度下限预设值",
                    "최소 밝기의 초기값"
                }
            },
            {
                "label.light_max_default", new []
                {
                    "Max Default Value",
                    "明るさの上限の初期値",
                    "亮度上限預設值",
                    "亮度上限预设值",
                    "최대 밝기의 초기값"
                }
            },
            {
                "label.color_temp", new []
                {
                    "Color Temp",
                    "色温度",
                    "色溫",
                    "色温",
                    "색온도"
                }
            },
            {
                "label.saturation", new []
                {
                    "Saturation",
                    "彩度",
                    "飽和度",
                    "饱和度",
                    "채도"
                }
            },
            {
                "label.monochrome", new []
                {
                    "Monochrome",
                    "モノクロ化",
                    "單色化",
                    "单色化",
                    "흑백화(Monochrome)"
                }
            },
            {
                "label.monochrome_additive", new []
                {
                    "Monochrome Additive",
                    "モノクロの加算",
                    "Monochrome Additive",
                    "Monochrome Additive",
                    "흑백화 가산(Monochrome Additive)"
                 }
            },
            {
                "label.unlit", new []
                {
                    "Unlit",
                    "Unlit",
                    "Unlit",
                    "Unlit",
                    "Unlit"
                }
            },
            {
                "label.apply_settings_avatar", new []
                {
                    "Set current settings as default and apply to other avatars",
                    "現在の設定をデフォルトにして他のアバターに適用する",
                    "將目前的設定設為預設值並套用至其他 Avatar",
                    "将目前的设置作为默认设置并应用到其它 Avatar",
                    "현재 설정을 기본값으로 설정하고 다른 아바타에 적용"
                }
            },
            { "label.apply_settings_project", new []
                {
                    "Apply to all Unity projects",
                    "全てのUnityプロジェクトに適用する",
                    "套用至全部 Unity 專案",
                    "应用到所有 Unity 项目",
                    "모든 Unity 프로젝트에 적용"
                }
            },
            {
                "label.document", new []
                {
                    "Documentation",
                    "説明書",
                    "說明書",
                    "文档",
                    "도움말"
                }
            },

            ///////////////////////////////////////////////////////
            // 情報　info
            {
                "info.shader_must_select" , new []
                {
                    "Target shader must be selected",
                    "対象シェーダーを選択してください",
                    "必須選擇目標 Shader",
                    "必须选择目标 Shader",
                    "대상 셰이더를 선택해 주세요"
                }
            },
            {
                "info.generate", new []
                {
                    "Generate",
                    "生成",
                    "生成",
                    "生成",
                    "생성"
                }
            },
            {
                "info.re_generate", new []
                {
                    "Regenerate", 
                    "再生成",
                    "再生成",
                    "重新生成",
                    "다시 생성"
                }
            },
            {
                "info.process", new []
                {
                    "Processing",
                    "生成中",
                    "生成中",
                    "生成中",
                    "생성 중"
                }
            },
            {
                "info.complete", new []
                {
                    "Complete",
                    "生成終了",
                    "完成",
                    "完成",
                    "생성 완료"
                }
            },
            {
                "info.error", new []
                {
                    "Error",
                    "エラー",
                    "錯誤",
                    "错误",
                    "오류"
                }
            },
            {
                "info.save", new []
                {
                    "Save",
                    "保存",
                    "儲存",
                    "保存",
                    "저장"
                }
            },
            {
                "info.initial_val", new []
                {
                    "Default Values for Additional Settings",
                    "追加設定の初期値",
                    "追加設定的預設值",
                    "附加设置的预设值",
                    "추가 설정 초기값"
                }
            },
            {
                "info.save_location", new []
                {
                    "Save Location",
                    "アセットの保存場所",
                    "資源儲存位置",
                    "资源保存位置",
                    "저장 위치"
                }
            },
            {
                "info.cancelled", new []
                {
                    "Cancelled",
                    "キャンセルしました",
                    "已取消",
                    "已取消",
                    "취소했습니다"
                }
            },
            
            ///////////////////////////////////////////////////////
            // ヒント tip
            {
                "tip.select_avatar", new []
                { 
                    "Select the avatar to generate animations for",
                    "アニメーションを生成するアバターをセットしてください",
                    "選擇要為其生成動畫的 Avatar",
                    "选择要为其生成动画的 Avatar",
                    "애니메이션을 생성할 아바타를 선택해 주세요"
                }
            },
            {
                "tip.use_default", new []
                { 
                    "Use the light animation in the initial state",
                    "初期状態でライトのアニメーションを使用します",
                    "在預設狀態下使用亮度動畫",
                    "在初始状态下开启亮度控制",
                    "초기 상태에서 조명 애니메이션을 사용합니다"
                }
            },
            {
                "tip.save_value", new []
                { 
                    "Keep brightness changes in the avatar",
                    "明るさの変更をアバターに保持したままにします",
                    "保持 Avatar 的亮度變化",
                    "保持 Avatar 的亮度设置",
                    "아바타의 밝기 설정을 유지합니다"
                }
            },
            {
                "tip.override_min_max", new []
                {
                    "Override default avatar brightness with the lower and upper limit parameters below",
                    "デフォルトのアバターの明るさを下限上限設定パラメータで上書きします",
                    "使用此組件的上下限設定覆蓋 Avatar 原本的亮度上下限",
                    "使用此组件的亮度上下限设置覆盖 Avatar 原本材质的亮度上下限",
                    "아바타의 기본 최소/최대 밝기 설정을 설정한 값으로 덮어씁니다"
                }
            },
            {
                "tip.light_max", new []
                { 
                    "Brightness upper limit setting",
                    "明るさの上限設定です",
                    "亮度的上限設定",
                    "亮度的上限设置",
                    "밝기의 최대값을 설정합니다"
                }
            },
            {
                "tip.light_min", new []
                { 
                    "Brightness lower limit setting",
                    "明るさの下限設定です",
                    "亮度的下限設定",
                    "亮度的下限设置",
                    "밝기의 최소값을 설정합니다"
                }
            },
            {
                "tip.light_default", new []
                { 
                    "Initial brightness setting",
                    "初期の明るさ設定",
                    "預設的亮度設定",
                    "预设的亮度设置",
                    "초기 밝기값 설정"
                }
            },
            {
                "tip.target_shader", new []
                { 
                    "Selects which shader(s) to control",
                    "制御するシェーダーを選択できます",
                    "選擇要控制哪個或哪些著色器",
                    "选择要控制哪些着色器",
                    "제어할 셰이더를 선택할 수 있습니다"
                }
            },
            {
                "tip.allow_color_tmp", new []
                { 
                    "Enables color temperature adjustment functionality",
                    "色温度の調節機能を有効化することができます",
                    "啟用色溫調整功能",
                    "启用色温调整功能",
                    "색온도 조정 기능을 활성화할 수 있습니다"
                }
            },
            {
                "tip.allow_saturation", new []
                { 
                    "Enables saturation adjustment functionality",
                    "彩度の調整機能を有効化することができます",
                    "啟用飽和度調整功能",
                    "启用饱和度调整功能",
                    "채도 조정 기능을 활성화할 수 있습니다"
                }
            },
            {
                "tip.allow_monochrome", new []
                { 
                    "Enables monochrome adjustment functionality",
                    "ライトのモノクロ化の調整機能を有効化することができます",
                    "啟用單色化調整功能",
                    "启用单色化调整功能",
                    "조명의 흑백화(Monochrome) 조정 기능을 활성화할 수 있습니다"
                }
            },
            {
                "tip.allow_unlit", new []
                {
                    "Enables Unlit adjustment functionality (Liltoon/Sunao Only)",
                    "Unlit の調整機能を有効化することができます(Liltoon/Sunao Only)",
                    "啟用 Unlit 調整功能（僅 Liltoon/Sunao）",
                    "启用 Unlit 调整功能（仅 Liltoon/Sunao）",
                    "Unlit 조정 기능을 할성화할 수 있습니다 (lilToon/Sunao 전용)"
                }
            },
            {
                "tip.allow_emission", new []
                {
                    "Enables Emission adjustment functionality (lilToon Only)",
                    "エミッションの調整機能を有効化することができます (lilToon Only)",
                    "啟用自發光調整功能（僅 Liltoon）",
                    "启用自发光调整功能（仅 Liltoon）",
                    "발광(Emission) 조정 기능을 활성화할 수 있습니다 (lilToon 전용)"
                }
            },
            {
                "tip.allow_reset", new []
                {
                    "Adds a reset button to return parameters to selected values",
                    "パラメータを設定値に戻すリセットボタンを追加します",
                    "新增一個重置按鈕以回到預設值",
                    "添加一个重置按钮用于重置设置为预设值",
                    "파라미터를 설정값으로 되돌리는 초기화 버튼을 추가합니다"
                }
            },
            {
                "tip.allow_override_poiyomi", new[]
                {
                    "Automatically set animation flags at build time",
                    "ビルド時に自動的にアニメーションフラグをセットします",
                    "在建置時自動設定動畫標誌",
                    "构建时自动设置 AnimationFlag",
                    "빌드 시 자동으로 애니메이션 플래그를 설정합니다"
                }
            },
            {
                "tip.allow_editor_only", new []
                {
                    "Exclude objects marked with EditorOnly tag from animations",
                    "EditorOnlyタグに設定されているオブジェクトをアニメーションから除外します",
                    "從動畫中排除標記為 EditorOnly 的對象",
                    "从动画中排除标记为 EditorOnly 的对象",
                    "태그가 EditorOnly로 설정되어 있는 오브젝트를 애니메이션에서 제외합니다"
                }
            },
            {
                "tip.allow_gen_playmode", new []
                {
                    "Automatically generate animations at build/play mode",
                    "ビルド・実行時にアニメーションを自動生成します",
                    "在播放模式或建置時自動生成動畫",
                    "在构建或播放模式时自动生成动画",
                    "빌드 및 PlayMode 진입 시 애니메이션을 자동으로 생성합니다"
                }
            },
            
            ///////////////////////////////////////////////////////
            // ウインドウ　window
            {
                "window.info.deprecated", new []
                {
                    "The settings window has been deprecated. See below for new ways of doing things.",
                    "設定ウィンドウは非推奨になりました。新しいやり方については以下をご覧ください。",
                    "這個設定介面已被棄用，請參閱下文以了解新方法。",
                    "这个设置界面已被弃用，请参阅下文以了解新方法。",
                    "설정 창은 권장되지 않습니다. 새로운 방법에 대해서는 아래를 참조해 주세요."
                }
            },
            {
                "Window.info.global_settings.changed", new []
                {
                    "Light Limit Changer global settings changed",
                    "Light Limit Changerのグローバル設定が変更されました",
                    "Light Limit Changer 全域設定已變更。",
                    "Light Limit Changer 全局设置已修改。",
                    "Light Limit Changer의 전역 설정이 변경되었습니다"
                }
            },
            {
                "Window.info.global_settings.update_available", new []
                {
                    "Update settings",
                    "設定を更新",
                    "更新設定",
                    "更新设置",
                    "설정 업데이트"
                }
            },
            {
                "Window.info.global_settings.title", new []
                {
                    "Loading Global Settings",
                    "グローバル設定の読み込み",
                    "加載全域設定",
                    "加载全局设置",
                    "전역 설정 가져오기"
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
                    @"有全局设置可用。要载入它吗?
按下取消将放弃载入全局设置并保留目前设置。",
                    @"전역 설정을 이용하실 수 있습니다. 불러올까요?
취소를 누르면 현재 설정을 유지합니다.",
                }
            },
            {
                "Window.info.gloabl_settings.save", new []
                {
                    "Save as Global Settings",
                    "グローバル設定として保存",
                    "儲存為全域設定",
                    "保存为全局设置",
                    "전역 설정으로 저장"
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
                    @"此功能会将当前设置应用于所有的其它 Unity 项目。
当前其它打开的 Unity 项目将在重启后加载全局设置。",
                    @"이 설정을 사용하면 다른 모든 Unity 프로젝트에 설정이 적용됩니다.
다른 프로젝트에서 설정을 불러오는 것은 Unity를 재시작한 이후부터 적용됩니다.",
                }
            },
            {
                "Window.info.choice.apply_save", new []
                {
                    "Apply as Global Settings",
                    "グローバル設定として適用",
                    "套用為全域設定",
                    "应用为全局设置",
                    "전역 설정으로 적용"
                }
            },
            {
                "Window.info.choice.apply_load", new []
                {
                    "Load Global Settings",
                    "グローバル設定を読み込む",
                    "載入全域設定",
                    "载入全局设置",
                    "전역 설정 불러오기"
                }
            },
            {
                "Window.info.error.already_setup", new []
                {
                    //[0] = Avatar Name
                    "Light Limit Changer has already been installed in the avatar \"{0}\"",
                    "アバター「{0}」にはすでにLight Limit Changerが導入されています",
                    "Avatar「{0}」已安裝 Light Limit Changer",
                    "Avatar “{0}” 已安装 Light Limit Changer",
                    "아바타「{0}」에는 이미 Light Limit Changer가 도입되어 있습니다"
                }
            },
            {
                "Window.info.cancel", new []
                {
                    "Cancel",
                    "キャンセル",
                    "取消",
                    "取消",
                    "취소"
                }
            },
            {
                "window.info.ok", new []
                {
                    "OK",
                    "OK",
                    "OK",
                    "OK",
                    "확인"
                }
            },
            ///////////////////////////////////////////////////////
            // Links
            {
                "link.document.changelog", new []
                {
                    "Light Limit Changer OfficialSite | Changelog",
                    "Light Limit Changer OfficialSite | 更新履歴",
                    "Light Limit Changer OfficialSite | 更新日誌",
                    "Light Limit Changer OfficialSite | 更新日志",
                    "Light Limit Changer OfficialSite | 변경 사항"
                }
            },
            {
                "link.document.recommend", new []
                {
                    "Light Limit Changer OfficialSite | Recommend Settings",
                    "Light Limit Changer OfficialSite | おすすめ設定",
                    "Light Limit Changer OfficialSite | 推薦設定",
                    "Light Limit Changer OfficialSite | 推荐设置",
                    "Light Limit Changer OfficialSite | 권장 설정"
                }
            },
            {
                "link.document.description", new []
                {
                    "Light Limit Changer OfficialSite | Description",
                    "Light Limit Changer OfficialSite | 設定概要",
                    "Light Limit Changer OfficialSite | 概述",
                    "Light Limit Changer OfficialSite | 概述",
                    "Light Limit Changer OfficialSite | 설정 개요"
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
如果材質顏色顯示異常，請不要使用這些功能並向作者報告。",
                @"已启用色温或饱和度控制功能。

此功能会在运行时以非破坏性方式修改材质和纹理，这可能会导致不可预知的错误并增加纹理显存消耗。
如果材质颜色显示异常，请不要使用这些功能并向作者报告错误。",
                @"색온도 또는 채도 변경이 활성화되어 있습니다.

이 기능은 실행 시 비파괴로 머티리얼 텍스처를 변경하기 때문에 잠재적인 버그나 텍스처 메모리가 증가할 수 있습니다.
머티리얼 색이 이상해진 경우에는 이 기능을 사용하지 말고, 제작자에게 버그 리포트를 부탁드립니다."
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
請使用最新版的 Poiyomi Shader。",
                    @"检测到 Poiyomi Shader 7.3。
请使用最新版的 Poiyomi Shader。",
                    @"Poiyomi Shader의 버전이 7.3으로 확인되었습니다.
최신 버전의 Poiyomi Shader로 업데이트 해야 기능을 정상적으로 사용하실 수 있습니다."
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
如果你不是有意的，請檢查你的設定。",
                    @"菜单未生成，因为没有动画目标。
如果你不是有意的，请检查你的设置。",
                    @"애니메이션 생성 대상이 존재하지 않아 메뉴를 생성하지 않았습니다.
만약 의도한 상황이 아닌 경우, 설정을 확인해 주세요."
                }
            },
            
            /////////////////////////////////////////////////
            // ExpressionMenu
            {
                "ExpressionMenu.light", new []
                {
                    "Light",
                    "明るさ",
                    "亮度",
                    "亮度",
                    "밝기"
                }
            },
            {
                "ExpressionMenu.light_min", new []
                {
                    "Min Light",
                    "明るさの下限",
                    "亮度下限",
                    "亮度下限",
                    "최소 밝기"
                }
            },
            {
                "ExpressionMenu.light_max", new []
                {
                    "Max Light",
                    "明るさの上限",
                    "亮度上限",
                    "亮度上限",
                    "최대 밝기"
                }
            },
            {
                "ExpressionMenu.color_temp", new []
                {
                    "Color Temp",
                    "色温度",
                    "色溫",
                    "色温",
                    "색온도"
                }
            },
            {
                "ExpressionMenu.saturation", new []
                {
                    "Saturation",
                    "彩度",
                    "飽和度",
                    "饱和度",
                    "채도"
                }
            },
            {
                "ExpressionMenu.unlit", new []
                {
                    "Unlit",
                    "Unlit",
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
                    "單色化",
                    "单色化",
                    "흑백화"
                }
            },
            {
                "ExpressionMenu.monochrome_additive", new []
                {
                    "Monochrome Additive",
                    "モノクロの加算",
                    "Monochrome Additive",
                    "Monochrome Additive",
                    "흑백화 가산"
                }
            },
            {
                "ExpressionMenu.emission", new []
                {
                    "Emission",
                    "エミッション",
                    "自發光",
                    "自发光",
                    "발광"
                }
            },
            {
                "ExpressionMenu.Enable", new []
                {
                    "Enable",
                    "有効",
                    "啟用",
                    "启用",
                    "활성화"
                }
            },
            {
                "ExpressionMenu.reset", new []
                {
                    "Reset",
                    "リセット",
                    "重置",
                    "重置",
                    "초기화"
                }
            },
            {
                "ExpressionMenu.Control", new []
                {
                    "Control",
                    "コントロール",
                    "控制",
                    "控制",
                    "추가 제어"
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
