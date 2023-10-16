using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;

namespace io.github.azukimochi
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Modular Avatar/Light Limit Changer")]
    public sealed class LightLimitChangerSettings : TagComponent, ISerializationCallbackReceiver
    {
        public LightLimitChangerParameters Parameters = LightLimitChangerParameters.Default;

        public List<UnityEngine.Object> Excludes = new List<UnityEngine.Object>();

#pragma warning disable CS0612
        public void OnAfterDeserialize()
        {
            if (Parameters.TargetShader != 0)
            {
                Debug.Log($"Convert new TargetShader selecting.");
                RuntimeShaderInfo.FromBitMask(ref Parameters.TargetShaders, Parameters.TargetShader);
                Parameters.TargetShader = 0;
            }
        }
#pragma warning restore CS0612

        public void OnBeforeSerialize()
        {
            // nop....
        }
    }
}