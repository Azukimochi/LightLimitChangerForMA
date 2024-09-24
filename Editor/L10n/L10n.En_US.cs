namespace io.github.azukimochi;

partial class L10n
{
    private class En_US : L10n
    {
        protected override string DisplayName => "English (United States)";
        protected override string Code => "en-US";

        public override string Category_Select_Avatar
            => @"Select Avatar";

        public override string Category_Select_Parameter
            => @"Parameter";

        public override string Category_Select_Option
            => @"Options";

        public override string Category_Select_Advanced
            => @"Advanced Settings";

        public override string Category_Save_Settings
            => @"Save Settings";

        public override string Label_Avatar
            => @"Avatar";

        public override string Label_Category_General_Settings
            => @"General Settings";

        public override string Label_Category_Additional_Settings
            => @"Additional Settings";

        public override string Label_Use_Default
            => @"Apply on Initial State";

        public override string Label_Save_Value
            => @"Save Parameter State";

        public override string Label_Override_Min_Max
            => @"Overwrite Default Min/Max";

        public override string Label_Light_Max
            => @"Max Brightness";

        public override string Label_Light_Min
            => @"Min Brightness";

        public override string Label_Light_Default
            => @"Default Brightness";

        public override string Label_Changelog
            => @"Changelog";

        public override string Label_Target_Shader
            => @"Target Shader";

        public override string Label_Allow_Color_Tmp
            => @"Enable Color Temperature Control";

        public override string Label_Allow_Saturation
            => @"Enable Saturation Control";

        public override string Label_Allow_Monochrome
            => @"Enable Monochrome Control";

        public override string Label_Allow_Unlit
            => @"Enable Unlit Control";

        public override string Label_Allow_Emission
            => @"Enable Emission Control";

        public override string Label_Allow_Reset
            => @"Add Reset Button";

        public override string Label_Allow_Override_Poiyomi
            => @"Enable Override Poiyomi AnimatedFlag";

        public override string Label_Allow_Editor_Only
            => @"Exclude EditorOnly";

        public override string Label_Allow_Gen_Playmode
            => @"Generate At Build/PlayMode";

        public override string Label_Excludes
            => @"Exclusion Settings";

        public override string Label_Grouping_Additional_Controls
            => @"Group Additional Controls";

        public override string Label_Separate_Light_Control
            => @"Set Min/Max Brightness Individually";

        public override string Label_Match_Avatar
            => @"Match Avatar";

        public override string Label_Separate_Light_Control_Init_Val
            => @"Individual Initial Value Settings";

        public override string Label_Light_Min_Default
            => @"Min Default Value";

        public override string Label_Light_Max_Default
            => @"Max Default Value";

        public override string Label_Color_Temp
            => @"Color Temp";

        public override string Label_Saturation
            => @"Saturation";

        public override string Label_Monochrome
            => @"Monochrome";

        public override string Label_Unlit
            => @"Unlit";

        public override string Label_Apply_Settings_Avatar
            => @"Set current settings as default and apply to other avatars";

        public override string Label_Apply_Settings_Project
            => @"Apply to all Unity projects";

        public override string Label_Document
            => @"Documentation";

        public override string Info_Shader_Must_Select
            => @"Target shader must be selected";

        public override string Info_Generate
            => @"Generate";

        public override string Info_Re_Generate
            => @"Regenerate";

        public override string Info_Process
            => @"Processing";

        public override string Info_Complete
            => @"Complete";

        public override string Info_Error
            => @"Error";

        public override string Info_Save
            => @"Save";

        public override string Info_Initial_Val
            => @"Default Values for Additional Settings";

        public override string Info_Save_Location
            => @"Save Location";

        public override string Info_Cancelled
            => @"Cancelled";

        public override string Tip_Select_Avatar
            => @"Select the avatar to generate animations for";

        public override string Tip_Use_Default
            => @"Use the light animation in the initial state";

        public override string Tip_Save_Value
            => @"Keep brightness changes in the avatar";

        public override string Tip_Override_Min_Max
            => @"Override default avatar brightness with the lower and upper limit parameters below";

        public override string Tip_Light_Max
            => @"Brightness upper limit setting";

        public override string Tip_Light_Min
            => @"Brightness lower limit setting";

        public override string Tip_Light_Default
            => @"Initial brightness setting";

        public override string Tip_Target_Shader
            => @"Selects which shader(s) to control";

        public override string Tip_Allow_Color_Tmp
            => @"Enables color temperature adjustment functionality";

        public override string Tip_Allow_Saturation
            => @"Enables saturation adjustment functionality";

        public override string Tip_Allow_Monochrome
            => @"Enables monochrome adjustment functionality";

        public override string Tip_Allow_Unlit
            => @"Enables Unlit adjustment functionality (Liltoon/Sunao Only)";

        public override string Tip_Allow_Emission
            => @"Enables Emission adjustment functionality (lilToon Only)";

        public override string Tip_Allow_Reset
            => @"Adds a reset button to return parameters to selected values";

        public override string Tip_Allow_Override_Poiyomi
            => @"Automatically set animation flags at build time";

        public override string Tip_Allow_Editor_Only
            => @"Exclude objects marked with EditorOnly tag from animations";

        public override string Tip_Allow_Gen_Playmode
            => @"Automatically generate animations at build/play mode";

        public override string Window_Info_Deprecated
            => @"The settings window has been deprecated. See below for new ways of doing things.";

        public override string Window_Info_Global_Settings_Changed
            => @"Light Limit Changer global settings changed";

        public override string Window_Info_Global_Settings_Update_Available
            => @"Update settings";

        public override string Window_Info_Global_Settings_Title
            => @"Loading Global Settings";

        public override string Window_Info_Global_Settings_Message
            => @"Global settings are available. Would you like to load them?
Pressing Cancel will discard the global settings and keep the current settings. ";

        public override string Window_Info_Gloabl_Settings_Save
            => @"Save as Global Settings";

        public override string Window_Info_Global_Settings_Save_Message
            => @"Using this setting will apply the setting to all other Unity projects.
Settings will be loaded in another project only after Unity is restarted.";

        public override string Window_Info_Choice_Apply_Save
            => @"Apply as Global Settings";

        public override string Window_Info_Choice_Apply_Load
            => @"Load Global Settings";

        public override string Window_Info_Error_Already_Setup
            => @"Light Limit Changer has already been installed in the avatar ""{0}""";

        public override string Window_Info_Cancel
            => @"Cancel";

        public override string Window_Info_Ok
            => @"OK";

        public override string Link_Document_Changelog
            => @"Light Limit Changer OfficialSite | Changelog";

        public override string Link_Document_Recommend
            => @"Light Limit Changer OfficialSite | Recommend Settings";

        public override string Link_Document_Description
            => @"Light Limit Changer OfficialSite | Description";

        public override string NDMF_Info_Usecolortemporsaturation
            => @"Color temperature and/or saturation controls are enabled.

This feature non-destructively modifies materials and textures at runtime, which may cause unexpected bugs and increase texture memory consumption.
If material colors appear abnormal, please disable these controls and file a bug report.";

        public override string NDMF_Info_Poiyomi_Old_Version
            => @"Poiyomi Shader version 7.3 is detected.
Please update to the latest version of Poiyomi Shader.";

        public override string NDMF_Info_Non_Generated
            => @"The menu was not generated because there was no animation target.
Please check your settings if this is not what you intended.";

        public override string ExpressionMenu_Light
            => @"Light";

        public override string ExpressionMenu_Light_Min
            => @"Min Light";

        public override string ExpressionMenu_Light_Max
            => @"Max Light";

        public override string ExpressionMenu_Color_Temp
            => @"Color Temp";

        public override string ExpressionMenu_Saturation
            => @"Saturation";

        public override string ExpressionMenu_Unlit
            => @"Unlit";

        public override string ExpressionMenu_Monochrome
            => @"Monochrome";

        public override string ExpressionMenu_Emission
            => @"Emission";

        public override string ExpressionMenu_Enable
            => @"Enable";

        public override string ExpressionMenu_Reset
            => @"Reset";

        public override string ExpressionMenu_Control
            => @"Control";
    }
}