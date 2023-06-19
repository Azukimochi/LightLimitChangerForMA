# LightLimitChanger
### これは何
Liltoonの明るさの下限/上限のアニメーションを生成するツールです。  
MAで使用できるPrefab形式で生成するため、非破壊かつアバターのFXへ変更を加えません。

### 使い方

＊Modular Avaterを前提としています  
＊Liltoonにのみ対応しています  

【起動方法】
Unity上部メニュー
ツール（Tools）→ Modular Avater → LightLimitChangerより起動します。

【使い方】
Avaterの欄にアバターをドラッグ＆ドロップします。

パラメータを設定し、[Generate/生成]ボタンを押すことで、"初回のみ" Assetsの保存先を指定するダイアログが表示され、アバターに[Light Limit Changer]オブジェクトが追加されます。

*2回目以降は保存ダイアログが表示されませんがアニメーションの内容は更新されています。

【アバターからの削除方法】
アバター内に追加されたLight Limit Changerオブジェクトを削除

【パラメータの解説】
DefaultUse：初期状態で適用状態か否か
SaveValue：アバターにパラメータを保持するか
MaxLight：明るさメニューの上限値
MinLight：明るさメニューの下限値
DefaultLight：明るさの初期値

![image](https://github.com/Azukimochi/LightLimitChangerForMA/assets/103747350/6101720b-8726-4539-b7be-c15a1b6f7e0d)



 
