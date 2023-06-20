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

        public bool AllowSaturationControl;
        public bool AddResetButton;

        public static LightLimitChangeParameters Default => new LightLimitChangeParameters()
        {
            IsDefaultUse = false,
            IsValueSave = false,
            DefaultLightValue = 0.5f,
            MaxLightValue = 1.0f,
            MinLightValue = 0.0f,
            AllowSaturationControl = false,
            AddResetButton = false,
        };
    }
}
