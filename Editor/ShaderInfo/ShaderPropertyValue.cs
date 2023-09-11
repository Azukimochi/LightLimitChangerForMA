using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace io.github.azukimochi
{
    [StructLayout(LayoutKind.Explicit)]
#pragma warning disable CS0660
#pragma warning disable CS0661
    internal readonly struct ShaderPropertyValue
#pragma warning restore CS0660
#pragma warning restore CS0661
    {
        public ShaderPropertyValue(ShaderPropertyType type)
        {
            this = default;
            Type = type;
        }

        public ShaderPropertyValue(ShaderPropertyType type, float value)
        {
            this = default;
            Type = type;
            Float = value;
        }

        public ShaderPropertyValue(ShaderPropertyType type, Color value)
        {
            this = default;
            Type = type;
            Color = value;
        }

        public ShaderPropertyValue(ShaderPropertyType type, Vector4 value)
        {
            this = default;
            Type = type;
            Vector = value;
        }

        [FieldOffset(0)]
        public readonly ShaderPropertyType Type;

        [FieldOffset(sizeof(ShaderPropertyType))]
        public readonly float Float;

        [FieldOffset(sizeof(ShaderPropertyType))]
        public readonly Color Color;

        [FieldOffset(sizeof(ShaderPropertyType))]
        public readonly Vector4 Vector;

        public override string ToString()
        {
            switch (Type)
            {
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    return Float.ToString();
                case ShaderPropertyType.Color: return Color.ToString();
                case ShaderPropertyType.Vector: return Vector.ToString();
                default: return null;
            }
        }

        public static bool operator ==(in ShaderPropertyValue left, float right)
            => (left.Type == ShaderPropertyType.Float || left.Type == ShaderPropertyType.Range) && left.Float == right;

        public static bool operator !=(in ShaderPropertyValue left, float right) => !(left == right);

        public static bool operator ==(in ShaderPropertyValue left, Color right)
            => left.Type == ShaderPropertyType.Color && left.Color == right;

        public static bool operator !=(in ShaderPropertyValue left, Color right) => !(left == right);

        public static bool operator ==(in ShaderPropertyValue left, Vector4 right)
            => left.Type == ShaderPropertyType.Vector && left.Vector == right;

        public static bool operator !=(in ShaderPropertyValue left, Vector4 right) => !(left == right);
    }
}
