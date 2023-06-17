using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace io.github.azukimochi
{
    public sealed class LightLimitChangerSettings : MonoBehaviour, IEditorOnly
    {
        public UnityEngine.Object FX;
        public AnimationClip DefaultAnimation;
        public AnimationClip ChangeLimitAnimation;

        public bool IsValid()
        {
            return FX != null
                && DefaultAnimation != null
                && ChangeLimitAnimation != null;
        }
    }
}
