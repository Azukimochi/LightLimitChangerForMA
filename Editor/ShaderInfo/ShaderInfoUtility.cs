using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace io.github.azukimochi
{
    internal static class ShaderInfoUtility
    {
        public const string MaterialAnimationKeyPrefix = "material.";

        public static AnimationClip SetParameterAnimation(this AnimationClip animationClip, in ControlAnimationParameters parameters, string propertyName, float value)
            => animationClip.SetParameterAnimation(parameters, propertyName, Utils.Animation.Constant(value));

        public static AnimationClip SetParameterAnimation(this AnimationClip animationClip, in ControlAnimationParameters parameters, string propertyName, float start, float end)
            => animationClip.SetParameterAnimation(parameters, propertyName, Utils.Animation.Linear(start, end));

        public static AnimationClip SetParameterAnimation(this AnimationClip animationClip, in ControlAnimationParameters parameters, string propertyName, float start, float mid, float end)
            => animationClip.SetParameterAnimation(parameters, propertyName, Utils.Animation.Linear(start, mid, end));


        public static AnimationClip SetParameterAnimation(this AnimationClip animationClip, in ControlAnimationParameters parameters, string propertyName, AnimationCurve curve)
        {
            animationClip.SetCurve(parameters.TargetPath, parameters.TargetType, $"{MaterialAnimationKeyPrefix}{propertyName}", curve);
            return animationClip;
        }

        public static AnimationClip SetParameterAnimation(this AnimationClip animationClip, in ControlAnimationParameters parameters, string propertyName, Vector4 value, IncludeField includeField = IncludeField.XYZW)
        {
            if (includeField.HasFlag(IncludeField.X))
            {
                animationClip.SetCurve(parameters.TargetPath, parameters.TargetType, $"{MaterialAnimationKeyPrefix}{propertyName}.x", Utils.Animation.Constant(value.x));
            }
            if (includeField.HasFlag(IncludeField.Y))
            {
                animationClip.SetCurve(parameters.TargetPath, parameters.TargetType, $"{MaterialAnimationKeyPrefix}{propertyName}.y", Utils.Animation.Constant(value.y));
            }
            if (includeField.HasFlag(IncludeField.Z))
            {
                animationClip.SetCurve(parameters.TargetPath, parameters.TargetType, $"{MaterialAnimationKeyPrefix}{propertyName}.z", Utils.Animation.Constant(value.z));
            }
            if (includeField.HasFlag(IncludeField.W))
            {
                animationClip.SetCurve(parameters.TargetPath, parameters.TargetType, $"{MaterialAnimationKeyPrefix}{propertyName}.w", Utils.Animation.Constant(value.w));
            }
            return animationClip;
        }

        public static AnimationClip SetParameterAnimation(this AnimationClip animationClip, in ControlAnimationParameters parameters, string propertyName, Color value, IncludeField includeField = IncludeField.RGBA)
        {
            if (includeField.HasFlag(IncludeField.R))
            {
                animationClip.SetCurve(parameters.TargetPath, parameters.TargetType, $"{MaterialAnimationKeyPrefix}{propertyName}.r", Utils.Animation.Constant(value.r));
            }
            if (includeField.HasFlag(IncludeField.G))
            {
                animationClip.SetCurve(parameters.TargetPath, parameters.TargetType, $"{MaterialAnimationKeyPrefix}{propertyName}.g", Utils.Animation.Constant(value.g));
            }
            if (includeField.HasFlag(IncludeField.B))
            {
                animationClip.SetCurve(parameters.TargetPath, parameters.TargetType, $"{MaterialAnimationKeyPrefix}{propertyName}.b", Utils.Animation.Constant(value.b));
            }
            if (includeField.HasFlag(IncludeField.A))
            {
                animationClip.SetCurve(parameters.TargetPath, parameters.TargetType, $"{MaterialAnimationKeyPrefix}{propertyName}.a", Utils.Animation.Constant(value.a));
            }

            return animationClip;
        }

        public static AnimationClip SetColorTempertureAnimation(this AnimationClip animationClip, in ControlAnimationParameters parameters, string propertyName, Color? baseColor = null)
        {
            var color = baseColor ?? Color.white;

            animationClip.SetCurve(parameters.TargetPath, parameters.TargetType, $"{MaterialAnimationKeyPrefix}{propertyName}.r", Utils.Animation.Linear(color.r * 0.6f, color.r, color.r));
            animationClip.SetCurve(parameters.TargetPath, parameters.TargetType, $"{MaterialAnimationKeyPrefix}{propertyName}.g", Utils.Animation.Linear(color.g * 0.95f, color.g, color.g * 0.8f));
            animationClip.SetCurve(parameters.TargetPath, parameters.TargetType, $"{MaterialAnimationKeyPrefix}{propertyName}.b", Utils.Animation.Linear(color.b, color.b, color.b * 0.6f));
            animationClip.SetCurve(parameters.TargetPath, parameters.TargetType, $"{MaterialAnimationKeyPrefix}{propertyName}.a", Utils.Animation.Linear(color.a, color.a, color.a));

            return animationClip;
        }

        public static int ToBitMask(in this TargetShaders targetShaders)
        {
            if (targetShaders.IsEverything)
                return -1;

            if (targetShaders.Targets == null || targetShaders.Targets.Length == 0)
                return 0;

            int result = 0;
            var infos = ShaderInfo.RegisteredShaderInfoNames;
            for (int i = 0; i < infos.Length; i++)
            {
                if (targetShaders.Targets.Contains(infos[i]))
                {
                    result |= 1 << i;
                }
            }
            return result;
        }

        public static void FromBitMask(ref this TargetShaders targetShaders, int bitMask)
        {
            if (bitMask <= 0)
            {
                targetShaders = bitMask == 0 ? TargetShaders.None : TargetShaders.Everything;
                return;
            }

            List<string> list = new List<string>();
            var infos = ShaderInfo.RegisteredShaderInfoNames;
            for (int i = 0; i < infos.Length; i++)
            {
                if (bitMask.IsPop(i))
                    list.Add(infos[i]);
            }
            targetShaders.IsEverything = false;
            targetShaders.Targets = list.ToArray();
        }

        private static bool IsPop(this int value, int index)
        {
            return (value & (1 << index)) != 0;
        }

        [Flags]
        public enum IncludeField : byte
        {
            X = 1 << 0,
            Y = 1 << 1,
            Z = 1 << 2,
            W = 1 << 3,

            R = 1 << 0,
            G = 1 << 1,
            B = 1 << 2,
            A = 1 << 3,

            XYZ = X | Y | Z,
            XYZW = X | Y | Z | W,

            RGB = R | G | B,
            RGBA = R | G | B | A,
        }
    }
}
