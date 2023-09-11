using System;

namespace io.github.azukimochi
{
    public readonly struct ControlAnimationParameters
    {
        public readonly string TargetPath;
        public readonly Type TargetType;

        public readonly float MinLightValue;
        public readonly float MaxLightValue;

        public ControlAnimationParameters(string targetPath, Type targetType, float minLightValue, float maxLightValue)
        {
            TargetPath = targetPath;
            TargetType = targetType;
            MinLightValue = minLightValue;
            MaxLightValue = maxLightValue;
        }
    }
}
