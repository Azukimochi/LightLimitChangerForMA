namespace io.github.azukimochi;

partial class L10n
{
    private sealed class Zh_Hant : L10n
    {
        protected override string DisplayName => "中文（繁體）";

        public override string Category_Select_Avatar
            => @"選擇 Avatar";

        public override string Category_Select_Parameter
            => @"參數";

        public override string Category_Select_Option
            => @"可選設定";

        public override string Category_Select_Advanced
            => @"進階設定";

        public override string Category_Save_Settings
            => @"儲存設定";

        public override string Label_Avatar
            => @"Avatar";

        public override string Label_Category_General_Settings
            => @"一般設定";

        public override string Label_Category_Additional_Settings
            => @"追加設定";

        public override string Label_Use_Default
            => @"預設開啟使用";

        public override string Label_Save_Value
            => @"保持狀態";

        public override string Label_Override_Min_Max
            => @"覆蓋原本的上下限";

        public override string Label_Light_Max
            => @"亮度上限";

        public override string Label_Light_Min
            => @"亮度下限";

        public override string Label_Light_Default
            => @"預設亮度";

        public override string Label_Changelog
            => @"更新日誌";

        public override string Label_Target_Shader
            => @"目標 Shader";

        public override string Label_Allow_Color_Tmp
            => @"啟用色溫控制";

        public override string Label_Allow_Saturation
            => @"啟用飽和度控制";

        public override string Label_Allow_Monochrome
            => @"啟用單色化控制";

        public override string Label_Allow_Unlit
            => @"啟用 Unlit 控制";

        public override string Label_Allow_Emission
            => @"啟用自發光控制";

        public override string Label_Allow_Reset
            => @"新增重置按鈕";

        public override string Label_Allow_Override_Poiyomi
            => @"啟用覆蓋 Poiyomi AnimatedFlag";

        public override string Label_Allow_Editor_Only
            => @"排除 EditorOnly";

        public override string Label_Allow_Gen_Playmode
            => @"在播放模式或建置時生成";

        public override string Label_Excludes
            => @"排除設定";

        public override string Label_Grouping_Additional_Controls
            => @"追加選單組";

        public override string Label_Separate_Light_Control
            => @"分別設置亮度上下限";

        public override string Label_Match_Avatar
            => @"配合 Avatar";

        public override string Label_Separate_Light_Control_Init_Val
            => @"個別預設值設定";

        public override string Label_Light_Min_Default
            => @"亮度下限預設值";

        public override string Label_Light_Max_Default
            => @"亮度上限預設值";

        public override string Label_Color_Temp
            => @"色溫";

        public override string Label_Saturation
            => @"飽和度";

        public override string Label_Monochrome
            => @"單色化";

        public override string Label_Unlit
            => @"Unlit";

        public override string Label_Apply_Settings_Avatar
            => @"將目前的設定設為預設值並套用至其他 Avatar";

        public override string Label_Apply_Settings_Project
            => @"套用至全部 Unity 專案";

        public override string Label_Document
            => @"說明書";

        public override string Info_Shader_Must_Select
            => @"必須選擇目標 Shader";

        public override string Info_Generate
            => @"生成";

        public override string Info_Re_Generate
            => @"再生成";

        public override string Info_Process
            => @"生成中";

        public override string Info_Complete
            => @"完成";

        public override string Info_Error
            => @"錯誤";

        public override string Info_Save
            => @"儲存";

        public override string Info_Initial_Val
            => @"追加設定的預設值";

        public override string Info_Save_Location
            => @"資源儲存位置";

        public override string Info_Cancelled
            => @"已取消";

        public override string Tip_Select_Avatar
            => @"選擇要為其生成動畫的 Avatar";

        public override string Tip_Use_Default
            => @"在預設狀態下使用亮度動畫";

        public override string Tip_Save_Value
            => @"保持 Avatar 的亮度變化";

        public override string Tip_Override_Min_Max
            => @"使用此組件的上下限設定覆蓋 Avatar 原本的亮度上下限";

        public override string Tip_Light_Max
            => @"亮度的上限設定";

        public override string Tip_Light_Min
            => @"亮度的下限設定";

        public override string Tip_Light_Default
            => @"預設的亮度設定";

        public override string Tip_Target_Shader
            => @"選擇要控制哪個或哪些著色器";

        public override string Tip_Allow_Color_Tmp
            => @"啟用色溫調整功能";

        public override string Tip_Allow_Saturation
            => @"啟用飽和度調整功能";

        public override string Tip_Allow_Monochrome
            => @"啟用單色化調整功能";

        public override string Tip_Allow_Unlit
            => @"啟用 Unlit 調整功能（僅 Liltoon/Sunao）";

        public override string Tip_Allow_Emission
            => @"啟用自發光調整功能（僅 Liltoon）";

        public override string Tip_Allow_Reset
            => @"新增一個重置按鈕以回到預設值";

        public override string Tip_Allow_Override_Poiyomi
            => @"在建置時自動設定動畫標誌";

        public override string Tip_Allow_Editor_Only
            => @"從動畫中排除標記為 EditorOnly 的對象";

        public override string Tip_Allow_Gen_Playmode
            => @"在播放模式或建置時自動生成動畫";

        public override string Window_Info_Deprecated
            => @"這個設定介面已被棄用，請參閱下文以了解新方法。";

        public override string Window_Info_Global_Settings_Changed
            => @"Light Limit Changer 全域設定已變更。";

        public override string Window_Info_Global_Settings_Update_Available
            => @"更新設定";

        public override string Window_Info_Global_Settings_Title
            => @"加載全域設定";

        public override string Window_Info_Global_Settings_Message
            => @"全域設定為可用。要載入它嗎？
按下取消將放棄全域設定並保留目前設定。";

        public override string Window_Info_Gloabl_Settings_Save
            => @"儲存為全域設定";

        public override string Window_Info_Global_Settings_Save_Message
            => @"使用此設定將設置套用於其他所有的 Unity 專案。
在另一個專案中，設定僅在重啟 Unity 後才會被加載。";

        public override string Window_Info_Choice_Apply_Save
            => @"套用為全域設定";

        public override string Window_Info_Choice_Apply_Load
            => @"載入全域設定";

        public override string Window_Info_Error_Already_Setup
            => @"Avatar「{0}」已安裝 Light Limit Changer";

        public override string Window_Info_Cancel
            => @"取消";

        public override string Window_Info_Ok
            => @"OK";

        public override string Link_Document_Changelog
            => @"Light Limit Changer OfficialSite | 更新日誌";

        public override string Link_Document_Recommend
            => @"Light Limit Changer OfficialSite | 推薦設定";

        public override string Link_Document_Description
            => @"Light Limit Changer OfficialSite | 概述";

        public override string NDMF_Info_Usecolortemporsaturation
            => @"已啟用色溫或飽和度控制功能。

此功能在運行時會以非破壞性方式修改材質和貼圖，這可能會導致潛在的錯誤和增加貼圖記憶體消耗。
如果材質顏色顯示異常，請不要使用這些功能並向作者報告。";

        public override string NDMF_Info_Poiyomi_Old_Version
            => @"檢測到 Poiyomi Shader 7.3 版本。
請使用最新版的 Poiyomi Shader。";

        public override string NDMF_Info_Non_Generated
            => @"選單未生成，因為沒有要動畫的目標。
如果你不是有意的，請檢查你的設定。";

        public override string ExpressionMenu_Light
            => @"亮度";

        public override string ExpressionMenu_Light_Min
            => @"亮度下限";

        public override string ExpressionMenu_Light_Max
            => @"亮度上限";

        public override string ExpressionMenu_Color_Temp
            => @"色溫";

        public override string ExpressionMenu_Saturation
            => @"飽和度";

        public override string ExpressionMenu_Unlit
            => @"Unlit";

        public override string ExpressionMenu_Monochrome
            => @"單色化";

        public override string ExpressionMenu_Emission
            => @"自發光";

        public override string ExpressionMenu_Enable
            => @"啟用";

        public override string ExpressionMenu_Reset
            => @"重置";

        public override string ExpressionMenu_Control
            => @"控制";


    }
}