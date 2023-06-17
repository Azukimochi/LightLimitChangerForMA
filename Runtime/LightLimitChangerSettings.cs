using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDKBase;

namespace io.github.azukimochi
{
    public sealed class LightLimitChangerSettings : MonoBehaviour, IEditorOnly
    {
        public AnimatorController FX;
        public AnimationClip DefaultAnimation;
        public AnimationClip ChangeLimitAnimation;
    }
}
