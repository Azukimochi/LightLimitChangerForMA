using UnityEngine;

namespace io.github.azukimochi
{
    public readonly struct ControlAnimationContainer
    {
        public readonly LightLimitControlType ControlType;
        public readonly string Name;

        public readonly string ParameterName;
        public readonly string AnimationName;
        public readonly float DefaultValue;

        public readonly AnimationClip Default;
        public readonly AnimationClip Control;

        public readonly Texture2D Icon;

        public ControlAnimationContainer(LightLimitControlType controlType, string name, string animationName, string parameterName, float defaultValue, Texture2D icon, AnimationClip @default, AnimationClip control)
        {
            ControlType = controlType;
            Name = name;
            AnimationName = animationName;
            ParameterName = parameterName;
            DefaultValue = defaultValue;
            Default = @default;
            Control = control;
            Icon = icon;
        }

        public static ControlAnimationContainer Create(LightLimitControlType controlType, string name, string animationName, string parameterName, float defaultValue, Texture2D icon = null, AnimationClip defaultAnimation = null)
            => new ControlAnimationContainer(controlType, name, animationName, parameterName, defaultValue, icon, defaultAnimation ?? new AnimationClip() { name = $"{animationName} Default" }, new AnimationClip() { name = $"{animationName} Control" });

        public void AddTo(LightLimitChangerObjectCache cache)
        {
            cache.Register(Default);
            cache.Register(Control);
        }
    }
}
