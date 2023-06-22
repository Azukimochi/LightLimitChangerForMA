#if UNITY_EDITOR

using UnityEditor.Animations;
using UnityEngine;
using VRC.SDKBase;

namespace io.github.azukimochi
{
    [DisallowMultipleComponent]
    public sealed class LightLimitChangerSettings : MonoBehaviour, IEditorOnly
    {
        public AnimatorController FX;

        public LightLimitChangeParameters Parameters = LightLimitChangeParameters.Default;

        public bool IsValid()
        {
            return FX != null;
        }
    }
}

#endif