namespace io.github.azukimochi;

partial class L10n
{
    private sealed class Zh_Hans : L10n
    {
        protected override string DisplayName => "中文（简体）";

        public override string Category_Select_Avatar
            => @"选择 Avatar";

        public override string Category_Select_Parameter
            => @"参数";

        public override string Category_Select_Option
            => @"选项";

        public override string Category_Select_Advanced
            => @"高级设置";

        public override string Category_Save_Settings
            => @"保存设置";

        public override string Label_Avatar
            => @"Avatar";

        public override string Label_Category_General_Settings
            => @"常规设置";

        public override string Label_Category_Additional_Settings
            => @"附加设置";

        public override string Label_Use_Default
            => @"初始状态为启用";

        public override string Label_Save_Value
            => @"参数设为保存";

        public override string Label_Override_Min_Max
            => @"覆盖原本的亮度上下限";

        public override string Label_Light_Max
            => @"亮度上限";

        public override string Label_Light_Min
            => @"亮度下限";

        public override string Label_Light_Default
            => @"预设亮度";

        public override string Label_Changelog
            => @"更新日志";

        public override string Label_Target_Shader
            => @"目标 Shader";

        public override string Label_Allow_Color_Tmp
            => @"启用色温控制";

        public override string Label_Allow_Saturation
            => @"启用饱和度控制";

        public override string Label_Allow_Monochrome
            => @"启用单色化控制";

        public override string Label_Allow_Unlit
            => @"启用 Unlit 控制";

        public override string Label_Allow_Emission
            => @"启用自发光控制";

        public override string Label_Allow_Reset
            => @"添加重置按钮";

        public override string Label_Allow_Override_Poiyomi
            => @"启用 Poiyomi AnimatedFlag 覆盖";

        public override string Label_Allow_Editor_Only
            => @"排除 EditorOnly";

        public override string Label_Allow_Gen_Playmode
            => @"在构建或播放模式时生成";

        public override string Label_Excludes
            => @"排除设置";

        public override string Label_Grouping_Additional_Controls
            => @"添加菜单组";

        public override string Label_Separate_Light_Control
            => @"分别设置亮度上下限";

        public override string Label_Match_Avatar
            => @"匹配 Avatar";

        public override string Label_Separate_Light_Control_Init_Val
            => @"分别设置预设值";

        public override string Label_Light_Min_Default
            => @"亮度下限预设值";

        public override string Label_Light_Max_Default
            => @"亮度上限预设值";

        public override string Label_Color_Temp
            => @"色温";

        public override string Label_Saturation
            => @"饱和度";

        public override string Label_Monochrome
            => @"单色化";

        public override string Label_Unlit
            => @"Unlit";

        public override string Label_Apply_Settings_Avatar
            => @"将目前的设置作为默认设置并应用到其它 Avatar";

        public override string Label_Apply_Settings_Project
            => @"应用到所有 Unity 项目";

        public override string Label_Document
            => @"文档";

        public override string Info_Shader_Must_Select
            => @"必须选择目标 Shader";

        public override string Info_Generate
            => @"生成";

        public override string Info_Re_Generate
            => @"重新生成";

        public override string Info_Process
            => @"生成中";

        public override string Info_Complete
            => @"完成";

        public override string Info_Error
            => @"错误";

        public override string Info_Save
            => @"保存";

        public override string Info_Initial_Val
            => @"附加设置的预设值";

        public override string Info_Save_Location
            => @"资源保存位置";

        public override string Info_Cancelled
            => @"已取消";

        public override string Tip_Select_Avatar
            => @"选择要为其生成动画的 Avatar";

        public override string Tip_Use_Default
            => @"在初始状态下开启亮度控制";

        public override string Tip_Save_Value
            => @"保持 Avatar 的亮度设置";

        public override string Tip_Override_Min_Max
            => @"使用此组件的亮度上下限设置覆盖 Avatar 原本材质的亮度上下限";

        public override string Tip_Light_Max
            => @"亮度的上限设置";

        public override string Tip_Light_Min
            => @"亮度的下限设置";

        public override string Tip_Light_Default
            => @"预设的亮度设置";

        public override string Tip_Target_Shader
            => @"选择要控制哪些着色器";

        public override string Tip_Allow_Color_Tmp
            => @"启用色温调整功能";

        public override string Tip_Allow_Saturation
            => @"启用饱和度调整功能";

        public override string Tip_Allow_Monochrome
            => @"启用单色化调整功能";

        public override string Tip_Allow_Unlit
            => @"启用 Unlit 调整功能（仅 Liltoon/Sunao）";

        public override string Tip_Allow_Emission
            => @"启用自发光调整功能（仅 Liltoon）";

        public override string Tip_Allow_Reset
            => @"添加一个重置按钮用于重置设置为预设值";

        public override string Tip_Allow_Override_Poiyomi
            => @"构建时自动设置 AnimationFlag";

        public override string Tip_Allow_Editor_Only
            => @"从动画中排除标记为 EditorOnly 的对象";

        public override string Tip_Allow_Gen_Playmode
            => @"在构建或播放模式时自动生成动画";

        public override string Window_Info_Deprecated
            => @"这个设置界面已被弃用，请参阅下文以了解新方法。";

        public override string Window_Info_Global_Settings_Changed
            => @"Light Limit Changer 全局设置已修改。";

        public override string Window_Info_Global_Settings_Update_Available
            => @"更新设置";

        public override string Window_Info_Global_Settings_Title
            => @"加载全局设置";

        public override string Window_Info_Global_Settings_Message
            => @"有全局设置可用。要载入它吗?
按下取消将放弃载入全局设置并保留目前设置。";

        public override string Window_Info_Gloabl_Settings_Save
            => @"保存为全局设置";

        public override string Window_Info_Global_Settings_Save_Message
            => @"此功能会将当前设置应用于所有的其它 Unity 项目。
当前其它打开的 Unity 项目将在重启后加载全局设置。";

        public override string Window_Info_Choice_Apply_Save
            => @"应用为全局设置";

        public override string Window_Info_Choice_Apply_Load
            => @"载入全局设置";

        public override string Window_Info_Error_Already_Setup
            => @"Avatar “{0}” 已安装 Light Limit Changer";

        public override string Window_Info_Cancel
            => @"取消";

        public override string Window_Info_Ok
            => @"OK";

        public override string Link_Document_Changelog
            => @"Light Limit Changer OfficialSite | 更新日志";

        public override string Link_Document_Recommend
            => @"Light Limit Changer OfficialSite | 推荐设置";

        public override string Link_Document_Description
            => @"Light Limit Changer OfficialSite | 概述";

        public override string NDMF_Info_Usecolortemporsaturation
            => @"已启用色温或饱和度控制功能。

此功能会在运行时以非破坏性方式修改材质和纹理，这可能会导致不可预知的错误并增加纹理显存消耗。
如果材质颜色显示异常，请不要使用这些功能并向作者报告错误。";

        public override string NDMF_Info_Poiyomi_Old_Version
            => @"检测到 Poiyomi Shader 7.3。
请使用最新版的 Poiyomi Shader。";

        public override string NDMF_Info_Non_Generated
            => @"菜单未生成，因为没有动画目标。
如果你不是有意的，请检查你的设置。";

        public override string ExpressionMenu_Light
            => @"亮度";

        public override string ExpressionMenu_Light_Min
            => @"亮度下限";

        public override string ExpressionMenu_Light_Max
            => @"亮度上限";

        public override string ExpressionMenu_Color_Temp
            => @"色温";

        public override string ExpressionMenu_Saturation
            => @"饱和度";

        public override string ExpressionMenu_Unlit
            => @"Unlit";

        public override string ExpressionMenu_Monochrome
            => @"单色化";

        public override string ExpressionMenu_Emission
            => @"自发光";

        public override string ExpressionMenu_Enable
            => @"启用";

        public override string ExpressionMenu_Reset
            => @"重置";

        public override string ExpressionMenu_Control
            => @"控制";
    }
}