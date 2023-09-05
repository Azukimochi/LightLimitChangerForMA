using System;
using UnityEngine;
using VRC.SDKBase;

namespace io.github.azukimochi
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100010)]
    public sealed class LightLimitChangerSettings : MonoBehaviour, IEditorOnly
    {
        public RuntimeAnimatorController FX;
        public LightLimitChangerParameters Parameters = LightLimitChangerParameters.Default;

        public static Action<LightLimitChangerSettings> OnAwake;

        public bool IsValid()
        {
            return FX != null;
        }

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