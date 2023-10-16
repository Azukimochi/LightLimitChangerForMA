using System;
using UnityEngine;

namespace io.github.azukimochi
{
    [DisallowMultipleComponent]
    public sealed class LightLimitChangerPreset : TagComponent
    {
        public Parameter Light;
        public Parameter LightMin;
        public Parameter LightMax;

        public Parameter Saturation;
        public Parameter ColorTemperature;
        public Parameter Unlit;

        [Serializable]
        public struct Parameter
        {
            public bool Enable;
            public float Value;
        }
    }
}
