using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.github.azukimochi
{
    [Serializable]
    public struct LightLimitChangeParameters
    {
        public bool IsDefaultUse;
        public bool IsValueSave;
        public float DefaultLightValue;
        public float MaxLightValue;
        public float MinLightValue;

        public TargetShaders TargetShader;

        public bool AllowSaturationControl;
        public bool AddResetButton;

        public static LightLimitChangeParameters Default => new LightLimitChangeParameters()
        {
            IsDefaultUse = false,
            IsValueSave = false,
            DefaultLightValue = 0.5f,
            MaxLightValue = 1.0f,
            MinLightValue = 0.0f,
            TargetShader = unchecked((TargetShaders)uint.MaxValue),  //TargetShaders.lilToon | TargetShaders.Sunao | TargetShaders.Poiyomi,
            AllowSaturationControl = false,
            AddResetButton = false,
        };
    }
}
