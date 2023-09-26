using System;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;

namespace io.github.azukimochi
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Modular Avatar/Light Limit Changer")]
    public sealed class LightLimitChangerSettings : MonoBehaviour, IEditorOnly
    {
        public LightLimitChangerParameters Parameters = LightLimitChangerParameters.Default;
    }
}