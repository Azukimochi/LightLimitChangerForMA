using System.Collections.Generic;
using VRC.SDKBase;

namespace io.github.azukimochi
{

    [DisallowMultipleComponent]
    [AddComponentMenu("NDMF/Light Limit Changer")]
    public sealed class LightLimitChangerComponent : MonoBehaviour, IEditorOnly
    {
        // コンポーネント作成時のバージョン マイグレーションとかに使える？
        [HideInInspector]
        public float Version = 2;

        /// <summary>
        /// 基本設定
        /// </summary>
        public GeneralSettings General;

        // シェーダー固有の設定たち
        public LilToonSettings LilToon;
        public PoiyomiSettings Poiyomi;
        public UnlitWFSettings UnlitWF;

        /// <summary>
        /// 除外設定
        /// </summary>
        public List<GameObject> Excludes = new();

        /// <summary>
        /// WDの設定
        /// </summary>
        public WriteDefaultsSetting WriteDefaults = WriteDefaultsSetting.MatchAvatar;

        /// <summary>
        /// 対象シェーダー
        /// </summary>
        public SupportedShaders TargetShader = (SupportedShaders)(-1);
    }
}