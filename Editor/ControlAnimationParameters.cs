using System;

namespace io.github.azukimochi
{
    public readonly struct ControlAnimationParameters
    {
        public readonly string TargetPath;
        public readonly Type TargetType;

        public readonly float MinLightValue;
        public readonly float MaxLightValue;
        public readonly float DefaultMinLightValue;
        public readonly float DefaultMaxLightValue;

        public ControlAnimationParameters(string targetPath, Type targetType, float minLightValue, float maxLightValue, float defaultMinLightValue, float defaultMaxLightValue)
        {
            TargetPath = targetPath;
            TargetType = targetType;
            MinLightValue = minLightValue;
            MaxLightValue = maxLightValue;
            DefaultMinLightValue = defaultMinLightValue;
            DefaultMaxLightValue = defaultMaxLightValue;
        }
    }
}
