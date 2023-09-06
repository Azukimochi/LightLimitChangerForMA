using UnityEngine;
using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    public readonly struct ControlAnimationContainer
    {
        public readonly LightLimitControlType ControlType;
        public readonly string Name;

        public readonly AnimationClip Default;
        public readonly AnimationClip Control;

        public ControlAnimationContainer(LightLimitControlType controlType, string name, AnimationClip @default, AnimationClip control)
        {
            ControlType = controlType;
            Name = name;
            Default = @default;
            Control = control;
        }

        public static ControlAnimationContainer Create(LightLimitControlType controlType, string animationName)
            => new ControlAnimationContainer(controlType, animationName, new AnimationClip() { name = $"{animationName} Default" }, new AnimationClip() { name = $"{animationName} Control"});

        public void AddTo(Object asset)
        {
            Default.AddTo(asset);
            Control.AddTo(asset);
        }
    }
}
