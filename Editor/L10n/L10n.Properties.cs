namespace io.github.azukimochi;

partial class L10n
{
    protected abstract string DisplayName { get; }
    protected abstract string Code { get; }

    public abstract string Category_Select_Avatar { get; }

    public abstract string Category_Select_Parameter { get; }

    public abstract string Category_Select_Option { get; }

    public abstract string Category_Select_Advanced { get; }

    public abstract string Category_Save_Settings { get; }


    public abstract string Label_Avatar { get; }

    public abstract string Label_Category_General_Settings { get; }

    public abstract string Label_Category_Additional_Settings { get; }

    public abstract string Label_Use_Default { get; }

    public abstract string Label_Save_Value { get; }

    public abstract string Label_Override_Min_Max { get; }

    public abstract string Label_Light_Max { get; }

    public abstract string Label_Light_Min { get; }

    public abstract string Label_Light_Default { get; }

    public abstract string Label_Changelog { get; }

    public abstract string Label_Target_Shader { get; }

    public abstract string Label_Allow_Color_Tmp { get; }

    public abstract string Label_Allow_Saturation { get; }

    public abstract string Label_Allow_Monochrome { get; }

    public abstract string Label_Allow_Unlit { get; }

    public abstract string Label_Allow_Emission { get; }

    public abstract string Label_Allow_Reset { get; }

    public abstract string Label_Allow_Override_Poiyomi { get; }

    public abstract string Label_Allow_Editor_Only { get; }

    public abstract string Label_Allow_Gen_Playmode { get; }

    public abstract string Label_Excludes { get; }

    public abstract string Label_Grouping_Additional_Controls { get; }

    public abstract string Label_Separate_Light_Control { get; }

    public abstract string Label_Match_Avatar { get; }

    public abstract string Label_Separate_Light_Control_Init_Val { get; }

    public abstract string Label_Light_Min_Default { get; }

    public abstract string Label_Light_Max_Default { get; }

    public abstract string Label_Color_Temp { get; }

    public abstract string Label_Saturation { get; }

    public abstract string Label_Monochrome { get; }

    public abstract string Label_Unlit { get; }

    public abstract string Label_Apply_Settings_Avatar { get; }

    public abstract string Label_Apply_Settings_Project { get; }

    public abstract string Label_Document { get; }


    public abstract string Info_Shader_Must_Select { get; }

    public abstract string Info_Generate { get; }

    public abstract string Info_Re_Generate { get; }

    public abstract string Info_Process { get; }

    public abstract string Info_Complete { get; }

    public abstract string Info_Error { get; }

    public abstract string Info_Save { get; }

    public abstract string Info_Initial_Val { get; }

    public abstract string Info_Save_Location { get; }

    public abstract string Info_Cancelled { get; }


    public abstract string Tip_Select_Avatar { get; }

    public abstract string Tip_Use_Default { get; }

    public abstract string Tip_Save_Value { get; }

    public abstract string Tip_Override_Min_Max { get; }

    public abstract string Tip_Light_Max { get; }

    public abstract string Tip_Light_Min { get; }

    public abstract string Tip_Light_Default { get; }

    public abstract string Tip_Target_Shader { get; }

    public abstract string Tip_Allow_Color_Tmp { get; }

    public abstract string Tip_Allow_Saturation { get; }

    public abstract string Tip_Allow_Monochrome { get; }

    public abstract string Tip_Allow_Unlit { get; }

    public abstract string Tip_Allow_Emission { get; }

    public abstract string Tip_Allow_Reset { get; }

    public abstract string Tip_Allow_Override_Poiyomi { get; }

    public abstract string Tip_Allow_Editor_Only { get; }

    public abstract string Tip_Allow_Gen_Playmode { get; }


    public abstract string Window_Info_Deprecated { get; }

    public abstract string Window_Info_Global_Settings_Changed { get; }

    public abstract string Window_Info_Global_Settings_Update_Available { get; }

    public abstract string Window_Info_Global_Settings_Title { get; }

    public abstract string Window_Info_Global_Settings_Message { get; }

    public abstract string Window_Info_Gloabl_Settings_Save { get; }

    public abstract string Window_Info_Global_Settings_Save_Message { get; }

    public abstract string Window_Info_Choice_Apply_Save { get; }

    public abstract string Window_Info_Choice_Apply_Load { get; }

    public abstract string Window_Info_Error_Already_Setup { get; }

    public abstract string Window_Info_Cancel { get; }

    public abstract string Window_Info_Ok { get; }


    public abstract string Link_Document_Changelog { get; }

    public abstract string Link_Document_Recommend { get; }

    public abstract string Link_Document_Description { get; }

    public abstract string NDMF_Info_Usecolortemporsaturation { get; }

    public abstract string NDMF_Info_Poiyomi_Old_Version { get; }

    public abstract string NDMF_Info_Non_Generated { get; }

    public abstract string ExpressionMenu_Light { get; }

    public abstract string ExpressionMenu_Light_Min { get; }

    public abstract string ExpressionMenu_Light_Max { get; }

    public abstract string ExpressionMenu_Color_Temp { get; }

    public abstract string ExpressionMenu_Saturation { get; }

    public abstract string ExpressionMenu_Unlit { get; }

    public abstract string ExpressionMenu_Monochrome { get; }

    public abstract string ExpressionMenu_Emission { get; }

    public abstract string ExpressionMenu_Enable { get; }

    public abstract string ExpressionMenu_Reset { get; }

    public abstract string ExpressionMenu_Control { get; }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
internal sealed class KeyAttribute : Attribute
{
    public KeyAttribute(string key) => Key = key;

    public string Key { get; }
}