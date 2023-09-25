using System;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;

namespace io.github.azukimochi
{
    [DisallowMultipleComponent]
    public sealed class LightLimitChangerSettings : MonoBehaviour, IEditorOnly
    {
        public LightLimitChangerParameters Parameters = LightLimitChangerParameters.Default;
    }
}