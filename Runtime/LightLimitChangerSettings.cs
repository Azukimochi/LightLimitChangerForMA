#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDKBase;

namespace io.github.azukimochi
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10000)]
    public sealed class LightLimitChangerSettings : MonoBehaviour, IEditorOnly
    {
        public AnimatorController FX;
        public LightLimitChangeParameters Parameters = LightLimitChangeParameters.Default;

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
                obj.SetActive(true);
            }
        }

        private void Start()
        {
        }
    }
}

#endif