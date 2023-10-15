using UnityEngine;
using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    public readonly struct ControlAnimationContainer
    {
        public readonly LightLimitControlType ControlType;
        public readonly string Name;

        public readonly string ParameterName;
        public readonly float DefaultValue;

        public readonly AnimationClip Default;
        public readonly AnimationClip Control;

        public ControlAnimationContainer(LightLimitControlType controlType, string name, string parameterName, float defaultValue, AnimationClip @default, AnimationClip control)
        {
            ControlType = controlType;
            Name = name;
            ParameterName = parameterName;
            DefaultValue = defaultValue;
            Default = @default;
            Control = control;
        }

        public static ControlAnimationContainer Create(LightLimitControlType controlType, string animationName, string parameterName, float defaultValue)
            => new ControlAnimationContainer(controlType, animationName, parameterName, defaultValue, new AnimationClip() { name = $"{animationName} Default" }, new AnimationClip() { name = $"{animationName} Control"});

        public void AddTo(LightLimitChangerObjectCache cache)
        {
            cache.Register(Default);
            cache.Register(Control);
        }
    }
}
