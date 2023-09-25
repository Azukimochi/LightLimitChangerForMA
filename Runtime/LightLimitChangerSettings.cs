using System;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;

namespace io.github.azukimochi
{
    [DisallowMultipleComponent]
    public sealed class LightLimitChangerSettings : MonoBehaviour, IEditorOnly
    {
        [FormerlySerializedAs("FX")]
        public UnityEngine.Object AssetContainer;

        public LightLimitChangerParameters Parameters = LightLimitChangerParameters.Default;
    }
}