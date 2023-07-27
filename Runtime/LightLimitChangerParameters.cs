using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool AllowColorTemp;
        public bool AllowSaturationControl;
        public bool AllowUnlitControl;
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
            TargetShader = unchecked((Shaders)uint.MaxValue),  //TargetShaders.lilToon | TargetShaders.Sunao | TargetShaders.Poiyomi,
            AllowColorTemp = false,
            AllowSaturationControl = false,
            AllowUnlitControl = false,
            AddResetButton = false,
            GenerateAtBuild = true,
            ExcludeEditorOnly = true
        };
    }
}
