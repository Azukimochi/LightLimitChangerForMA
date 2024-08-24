using System;
using UnityEngine;

namespace io.github.azukimochi
{
    [Serializable]
    public class LightLimitChangerParameters
    {
        public bool IsDefaultUse  = false;
        public bool IsValueSave = false;
        public bool OverwriteDefaultLightMinMax = true;

        [Range(0, 1)]
        public float DefaultLightValue = 0.5f;

        [Range(0, 1)]
        public float DefaultMinLightValue = 0.0f;

        [Range(0, 1)]
        public float DefaultMaxLightValue = 1.0f;

        [Range(0, 10)]
        public float MaxLightValue = 1.0f;

        [Range(0, 10)]
        public float MinLightValue = 0.0f;

        [Obsolete] public int TargetShader = 0;

        public TargetShaders TargetShaders = TargetShaders.Everything;

        public bool AllowColorTempControl = false;
        public bool AllowSaturationControl = false;
        public bool AllowMonochromeControl = false;
        public bool AllowUnlitControl = false;
        public bool AllowEmissionControl = false;
        public bool AddResetButton = false;
        
        [Range(0,1)]
        public float InitialTempControlValue = 0.5f;

        [Range(0,1)]
        public float InitialSaturationControlValue = 0.5f;
        
        [Range(0,1)]
        public float InitialMonochromeControlValue = 0.0f;

        [Range(0,1)]
        public float MonochromeAdditiveLightingValue = 0.0f;

        [Range(0,1)]
        public float DefaultMonochromeLightingValue = 0.0f;

        [Range(0,1)]
        public float DefaultMonochromeAdditiveLightingValue = 0.0f;
        
        [Range(0,1)]
        public float InitialUnlitControlValue = 0.0f;
        
        public bool IsSeparateLightControl = false;
        public bool IsSeparateMonochromeControl = false;

        public bool IsGroupingAdditionalControls = false;
    }
}
