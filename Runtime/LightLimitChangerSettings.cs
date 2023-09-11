using System;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;

namespace io.github.azukimochi
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100010)]
    public sealed class LightLimitChangerSettings : MonoBehaviour, IEditorOnly
    {
        [FormerlySerializedAs("FX")]
        public UnityEngine.Object AssetContainer;

        public LightLimitChangerParameters Parameters = LightLimitChangerParameters.Default;

        public static Action<LightLimitChangerSettings> OnAwake;

        private void Awake()
        {
            if (Parameters.GenerateAtBuild)
            {
                var obj = this.gameObject;
                obj.SetActive(false);
                OnAwake?.Invoke(this);
                DestroyImmediate(this);
            }
        }
    }
}