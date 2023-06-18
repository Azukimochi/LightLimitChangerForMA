using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace io.github.azukimochi
{
    public sealed class LightLimitChangerSettings : MonoBehaviour, IEditorOnly
    {
        public UnityEngine.Object FX;

        public LightLimitChangeParameters Parameters = LightLimitChangeParameters.Default;

        public bool IsValid()
        {
            return FX != null
                // FX is UnityEditor.AnimatorController
                && FX.GetType().FullName == "UnityEditor.Animations.AnimatorController";
        }
    }
}
