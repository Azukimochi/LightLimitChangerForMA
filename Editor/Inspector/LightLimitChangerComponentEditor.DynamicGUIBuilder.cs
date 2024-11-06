using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using static VRC.Core.ApiVRChatProductDetails;

namespace io.github.azukimochi;

partial class LightLimitChangerComponentEditor
{
    internal static void OnGUI<TSettings>(TSettings settings, SerializedProperty property) where TSettings : ISettings
        => DynamicGUIBuilder<TSettings>.OnGUI(settings, property);

    internal static class DynamicGUIBuilder<TSettings> where TSettings : ISettings
    {
        public delegate void OnGUIDelegate(TSettings settings, SerializedProperty property);

        public static readonly OnGUIDelegate OnGUI;

        static DynamicGUIBuilder()
        {
            var method = new DynamicMethod($"{typeof(TSettings).Name}_OnGUI", null, typeof(OnGUIDelegate).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance).GetParameters().Select(x => x.ParameterType).ToArray());
            var il = method.GetILGenerator();

            StringBuilder sb = new();
            var local_getShowDescription = il.DeclareLocal(typeof(bool));
            var local_isAdvancedMode = il.DeclareLocal(typeof(bool));
            var local_range = il.DeclareLocal(typeof(Vector2?));
            var local_minMaxRange = il.DeclareLocal(typeof(Vector2?));

            // showDesc = LightLimitChangerComponentEditor.ShowDescriptions;
            il.GetProperty<LightLimitChangerComponentEditor>(nameof(ShowDescriptions));
            il.Stloc(local_getShowDescription);

            il.GetProperty<LightLimitChangerComponentEditor>(nameof(IsAdvancedMode));
            il.Stloc(local_isAdvancedMode);


            foreach (var field in default(TSettings).AllParameterFields())
            {
                var info = new ParameterFieldInfo(field);
                _ = nameof(ParameterDrawer.DrawLayout);

                void CreateNullableVector2(float min, float max)
                {
                    il.Float(min);
                    il.Float(max);
                    il.NewObj<Func<float, float, Vector2>>();
                    il.NewObj<Func<Vector2, Vector2?>>();
                }

                if (info.RangeAttribute is { } range)
                {
                    CreateNullableVector2(range.Min, range.Max);
                    il.Stloc(local_range);
                }
                else
                {
                    il.Ldloca(local_range);
                    il.Emit(OpCodes.Initobj, typeof(Vector2?));
                }

                if (info.MinMaxRangeAttribute is { } minmax)
                {
                    CreateNullableVector2(minmax.Min, minmax.Max);
                    il.Stloc(local_minMaxRange);
                }
                else
                {
                    il.Ldloca(local_minMaxRange);
                    il.Emit(OpCodes.Initobj, typeof(Vector2?));
                }

                il.Ldarg(1);
                il.Ldstr(field.Name);
                il.Call<SerializedProperty, Func<string, SerializedProperty>>(x => x.FindPropertyRelative);
                il.Ldstr(StringExt.Create(sb, $"settings:{SettingsFieldInfo<TSettings>.Id}/{char.ToLowerInvariant(field.Name[0])}{field.Name.AsSpan(1)}/label"));
                il.Call<Func<string, GUIContent>>(EditorGUIUtility.TrTempContent);
                il.Int(info.DisableInitialValueSliderAttribute is null ? 1 : 0);
                il.Ldloc(local_range);
                il.Ldloc(local_minMaxRange);
                il.Ldloc(local_isAdvancedMode);
                il.Call(ParameterDrawer.DrawLayout);

                il.Ldloc(local_getShowDescription);
                il.If(() =>
                {
                    il.Ldstr(StringExt.Create(sb, $"settings:{SettingsFieldInfo<TSettings>.Id}/{char.ToLowerInvariant(field.Name[0])}{field.Name.AsSpan(1)}/description"));
                    il.Int((int)MessageType.Info);
                    il.Call<Action<string, MessageType>>(EditorGUILayout.HelpBox);
                });

                il.Ldarg(1);
                il.GetProperty<SerializedProperty>(nameof(SerializedProperty.isExpanded));
                il.Ldloc(local_isAdvancedMode);
                il.Emit(OpCodes.And);
                il.If(() =>
                {
                    il.Call<Action>(DrawSeparator);
                });
            }
            il.Emit(OpCodes.Ret);

            OnGUI = method.CreateDelegate(typeof(OnGUIDelegate)) as OnGUIDelegate;
        }
    }
}
