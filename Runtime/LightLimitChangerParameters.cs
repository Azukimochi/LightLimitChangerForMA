using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Serialization;

namespace io.github.azukimochi
{
    [Serializable]
    public struct LightLimitChangerParameters
    {
        public bool IsDefaultUse;
        public bool IsValueSave;
        public bool OverwriteDefaultLightMinMax;
        public float DefaultLightValue;
        public float MaxLightValue;
        public float MinLightValue;

        public Shaders TargetShader;

        public bool AllowColorTempControl;
        public bool AllowSaturationControl;
        public bool AllowUnlitControl;
        public bool AllowOverridePoiyomiAnimTag;
        public bool AddResetButton;
        public bool GenerateAtBuild;
        public bool ExcludeEditorOnly;

        public static LightLimitChangerParameters Default => new LightLimitChangerParameters()
        {
            IsDefaultUse = false,
            IsValueSave = false,
            OverwriteDefaultLightMinMax = true,
            DefaultLightValue = 0.5f,
            MaxLightValue = 1.0f,
            MinLightValue = 0.0f,
            TargetShader = (Shaders)(-1),  //TargetShaders.lilToon | TargetShaders.Sunao | TargetShaders.Poiyomi,
            AllowColorTempControl = false,
            AllowSaturationControl = false,
            AllowUnlitControl = false,
            AllowOverridePoiyomiAnimTag = true,
            AddResetButton = false,
            GenerateAtBuild = true,
            ExcludeEditorOnly = true
        };
    }
}
