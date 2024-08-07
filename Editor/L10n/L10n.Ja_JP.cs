namespace io.github.azukimochi;

partial class L10n
{
    private class Ja_JP : L10n
    {
        protected override string DisplayName => "日本語 (日本)";


        public override string Category_Select_Avatar
            => @"アバターを選択";

        public override string Category_Select_Parameter
            => @"パラメーター";

        public override string Category_Select_Option
            => @"オプション";

        public override string Category_Select_Advanced
            => @"詳細設定";

        public override string Category_Save_Settings
            => @"設定を保存";

        public override string Label_Avatar
            => @"アバター";

        public override string Label_Category_General_Settings
            => @"一般設定";

        public override string Label_Category_Additional_Settings
            => @"追加設定";

        public override string Label_Use_Default
            => @"初期状態で適用する";

        public override string Label_Save_Value
            => @"パラメータを保持する";

        public override string Label_Override_Min_Max
            => @"初期の上限と下限を上書きする";

        public override string Label_Light_Max
            => @"明るさの上限";

        public override string Label_Light_Min
            => @"明るさの下限";

        public override string Label_Light_Default
            => @"明るさの初期値";

        public override string Label_Changelog
            => @"更新履歴";

        public override string Label_Target_Shader
            => @"対象シェーダー";

        public override string Label_Allow_Color_Tmp
            => @"色温度調整を有効にする";

        public override string Label_Allow_Saturation
            => @"彩度調整を有効にする";

        public override string Label_Allow_Monochrome
            => @"ライトのモノクロ化調整を有効にする";

        public override string Label_Allow_Unlit
            => @"Unlit調整を有効にする";

        public override string Label_Allow_Emission
            => @"エミッションの調整を有効にする";

        public override string Label_Allow_Reset
            => @"リセットボタンを追加する";

        public override string Label_Allow_Override_Poiyomi
            => @"PoiyomiのAnimatedフラグを上書きする";

        public override string Label_Allow_Editor_Only
            => @"EditorOnlyを除外する";

        public override string Label_Allow_Gen_Playmode
            => @"ビルド・実行時に生成する";

        public override string Label_Excludes
            => @"除外設定";

        public override string Label_Grouping_Additional_Controls
            => @"追加コントロールをグループ化する";

        public override string Label_Separate_Light_Control
            => @"明るさの上限と下限を別々に設定する";

        public override string Label_Match_Avatar
            => @"アバターに合わせる";

        public override string Label_Separate_Light_Control_Init_Val
            => @"個別の初期値設定";

        public override string Label_Light_Min_Default
            => @"明るさの下限の初期値";

        public override string Label_Light_Max_Default
            => @"明るさの上限の初期値";

        public override string Label_Color_Temp
            => @"色温度";

        public override string Label_Saturation
            => @"彩度";

        public override string Label_Monochrome
            => @"モノクロ化";

        public override string Label_Unlit
            => @"Unlit";

        public override string Label_Apply_Settings_Avatar
            => @"現在の設定をデフォルトにして他のアバターに適用する";

        public override string Label_Apply_Settings_Project
            => @"全てのUnityプロジェクトに適用する";

        public override string Label_Document
            => @"説明書";

        public override string Info_Shader_Must_Select
            => @"対象シェーダーを選択してください";

        public override string Info_Generate
            => @"生成";

        public override string Info_Re_Generate
            => @"再生成";

        public override string Info_Process
            => @"生成中";

        public override string Info_Complete
            => @"生成終了";

        public override string Info_Error
            => @"エラー";

        public override string Info_Save
            => @"保存";

        public override string Info_Initial_Val
            => @"追加設定の初期値";

        public override string Info_Save_Location
            => @"アセットの保存場所";

        public override string Info_Cancelled
            => @"キャンセルしました";

        public override string Tip_Select_Avatar
            => @"アニメーションを生成するアバターをセットしてください";

        public override string Tip_Use_Default
            => @"初期状態でライトのアニメーションを使用します";

        public override string Tip_Save_Value
            => @"明るさの変更をアバターに保持したままにします";

        public override string Tip_Override_Min_Max
            => @"デフォルトのアバターの明るさを下限上限設定パラメータで上書きします";

        public override string Tip_Light_Max
            => @"明るさの上限設定です";

        public override string Tip_Light_Min
            => @"明るさの下限設定です";

        public override string Tip_Light_Default
            => @"初期の明るさ設定";

        public override string Tip_Target_Shader
            => @"制御するシェーダーを選択できます";

        public override string Tip_Allow_Color_Tmp
            => @"色温度の調節機能を有効化することができます";

        public override string Tip_Allow_Saturation
            => @"彩度の調整機能を有効化することができます";

        public override string Tip_Allow_Monochrome
            => @"ライトのモノクロ化の調整機能を有効化することができます";

        public override string Tip_Allow_Unlit
            => @"Unlit の調整機能を有効化することができます(Liltoon/Sunao Only)";

        public override string Tip_Allow_Emission
            => @"エミッションの調整機能を有効化することができます (lilToon Only)";

        public override string Tip_Allow_Reset
            => @"パラメータを設定値に戻すリセットボタンを追加します";

        public override string Tip_Allow_Override_Poiyomi
            => @"ビルド時に自動的にアニメーションフラグをセットします";

        public override string Tip_Allow_Editor_Only
            => @"EditorOnlyタグに設定されているオブジェクトをアニメーションから除外します";

        public override string Tip_Allow_Gen_Playmode
            => @"ビルド・実行時にアニメーションを自動生成します";

        public override string Window_Info_Deprecated
            => @"設定ウィンドウは非推奨になりました。新しいやり方については以下をご覧ください。";

        public override string Window_Info_Global_Settings_Changed
            => @"Light Limit Changerのグローバル設定が変更されました";

        public override string Window_Info_Global_Settings_Update_Available
            => @"設定を更新";

        public override string Window_Info_Global_Settings_Title
            => @"グローバル設定の読み込み";

        public override string Window_Info_Global_Settings_Message
            => @"グローバル設定が利用可能です。読み込みますか？
キャンセルを押すとグローバル設定を破棄して現在の設定を保持します。";

        public override string Window_Info_Gloabl_Settings_Save
            => @"グローバル設定として保存";

        public override string Window_Info_Global_Settings_Save_Message
            => @"この設定を使用すると、他のすべてのUnityプロジェクトへ設定が適用されます。
別のプロジェクトで設定が読み込まれるのはUnity再起動後です。";

        public override string Window_Info_Choice_Apply_Save
            => @"グローバル設定として適用";

        public override string Window_Info_Choice_Apply_Load
            => @"グローバル設定を読み込む";

        public override string Window_Info_Error_Already_Setup
            => @"アバター「{0}」にはすでにLight Limit Changerが導入されています";

        public override string Window_Info_Cancel
            => @"キャンセル";

        public override string Window_Info_Ok
            => @"OK";

        public override string Link_Document_Changelog
            => @"Light Limit Changer OfficialSite | 更新履歴";

        public override string Link_Document_Recommend
            => @"Light Limit Changer OfficialSite | おすすめ設定";

        public override string Link_Document_Description
            => @"Light Limit Changer OfficialSite | 設定概要";

        public override string NDMF_Info_Usecolortemporsaturation
            => @"色温度または彩度の変更が有効になっています。

この機能は実行時に非破壊でマテリアル・テクスチャを変更するため、潜在的なバグやテクスチャメモリが増加する可能性があります。
マテリアルの色がおかしくなった場合にはこの機能を使用せず、作者に報告をお願いします。";

        public override string NDMF_Info_Poiyomi_Old_Version
            => @"Poiyomi Shader のバージョン 7.3 が検出されました。
Poiyomi Shaderの最新版へのアップデートをお願いします";

        public override string NDMF_Info_Non_Generated
            => @"アニメーションの生成対象が存在しなかったため、メニューを生成しませんでした。
これが意図したものでない場合には、設定を確認してください。";

        public override string ExpressionMenu_Light
            => @"明るさ";

        public override string ExpressionMenu_Light_Min
            => @"明るさの下限";

        public override string ExpressionMenu_Light_Max
            => @"明るさの上限";

        public override string ExpressionMenu_Color_Temp
            => @"色温度";

        public override string ExpressionMenu_Saturation
            => @"彩度";

        public override string ExpressionMenu_Unlit
            => @"Unlit";

        public override string ExpressionMenu_Monochrome
            => @"モノクロ化";

        public override string ExpressionMenu_Emission
            => @"エミッション";

        public override string ExpressionMenu_Enable
            => @"有効";

        public override string ExpressionMenu_Reset
            => @"リセット";

        public override string ExpressionMenu_Control
            => @"コントロール";
    }
}