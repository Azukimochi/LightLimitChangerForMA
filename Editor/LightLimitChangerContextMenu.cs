using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VRC.SDK3.Avatars.Components;

namespace io.github.azukimochi;

[InitializeOnLoad]
internal static class LightLimitChangerContextMenu
{
    private const string MenuRoot = "GameObject/Light Limit Changer/";
    private const int MenuPriority = 49;

    private const string SetupMenuPath = MenuRoot + "Setup";

    private static readonly MenuList presetMenus = new();

    static LightLimitChangerContextMenu()
    {
        MenuHelper.AddMenuItem(SetupMenuPath, null, false, MenuPriority, () => Setup(), () => Selection.gameObjects.Any(x => x.TryGetComponent<VRCAvatarDescriptor>(out _)));
        LightLimitChangerPrefab.UpdatePreset += (name, mode) =>
        {
            var path = MenuRoot + $"Preset/{name}";
            if (mode == LightLimitChangerPrefab.UpdatePresetMode.Append)
            {
                presetMenus.AddSeparator(MenuRoot + "PresetSeparator", MenuPriority + 100);
                presetMenus.Add(path, MenuPriority + 200, () => { Debug.Log(name);  });
            }
            else
            {
                presetMenus.Remove(path);
            }
        };
    }

    public static void Setup()
    {
        var selection = Selection.objects;
        foreach (ref var select in selection.AsSpan())
        {
            var avatarRoot = select as GameObject;
            if (avatarRoot == null || !avatarRoot.TryGetComponent<VRCAvatarDescriptor>(out var descripter))
            {
                select = null;
                continue;
            }

            if (avatarRoot.GetComponentInChildren<VRCAvatarDescriptor>(true) != null)
            {
                // TODO: 重複インストールの警告
            }

            var prefab = LightLimitChangerPrefab.DefaultSettings;
            if (prefab != null)
            {
                prefab = PrefabUtility.InstantiatePrefab(prefab, avatarRoot.transform) as GameObject;
                prefab.name = LightLimitChanger.Title;
            }
            else
            {
                prefab = new GameObject(LightLimitChanger.Title);
                prefab.AddComponent<LightLimitChangerComponent>();
                prefab.transform.parent = avatarRoot.transform;
            }
            
            select = prefab;

            EditorGUIUtility.PingObject(prefab);
        }
        Selection.objects = selection;
    }

    internal static class MenuHelper
    {
        private static HashSet<string> items = new();

        private delegate void AddMenuItemDelegate(string name, string shortcut, bool isChecked, int priority, Action execute, Func<bool> validate);
        private delegate void AddSeparatorDelegate(string name, int priority);
        private delegate void RemoveMenuItemDelegate(string name);
        private static readonly AddMenuItemDelegate _AddMenuItem;
        private static readonly AddSeparatorDelegate _AddSeparator;
        private static readonly RemoveMenuItemDelegate _RemoveMenuItem;
        private static readonly Action _Update;

        public static void AddMenuItem(string name, string shortcut, bool isChecked, int priority, Action execute, Func<bool> validate)
            => _AddMenuItem(name, shortcut, isChecked, priority, execute, validate);

        public static void AddSeparator(string name, int priority) 
            => _AddSeparator(name, priority);

        public static void RemoveMenuItem(string name) 
            => _RemoveMenuItem(name);

        public static void Update() 
            => _Update();

        static MenuHelper()
        {
            _AddMenuItem = DefineMethod<AddMenuItemDelegate>(nameof(AddMenuItem));
            _AddSeparator = DefineMethod<AddSeparatorDelegate>(nameof(AddSeparator));
            _RemoveMenuItem = DefineMethod<RemoveMenuItemDelegate>(nameof(RemoveMenuItem));

            static T DefineMethod<T>(string name) where T : Delegate
            {
                var original = typeof(Menu).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
                var method = new DynamicMethod(original.Name, null, typeof(T).GetMethod("Invoke").GetParameters().Select(x => x.ParameterType).ToArray(), typeof(Menu), true);

                var il = method.GetILGenerator();
                for (int i = 0; i < original.GetParameters().Length; i++)
                {
                    Ldarg(il, i);
                }
                il.Emit(OpCodes.Call, original);
                il.Emit(OpCodes.Ret);

                return method.CreateDelegate(typeof(T)) as T;
            }

            // Update
            {
                var internalUpdateAllMenus = typeof(EditorUtility).GetMethod("Internal_UpdateAllMenus", BindingFlags.Static | BindingFlags.NonPublic);
                var shortcutIntegrationType = Type.GetType("UnityEditor.ShortcutManagement.ShortcutIntegration, UnityEditor.CoreModule");
                var instanceProp = shortcutIntegrationType?.GetProperty("instance", BindingFlags.Static | BindingFlags.Public);
                var instance = instanceProp?.GetValue(null);
                var rebuildShortcutsMethod = instance?.GetType().GetMethod("RebuildShortcuts", BindingFlags.Instance | BindingFlags.NonPublic);

                var proxy = typeof(MenuHelper).GetMethod("Update", BindingFlags.Instance | BindingFlags.Public);
                var method = new DynamicMethod("Update", null, Type.EmptyTypes, true);

                var il = method.GetILGenerator();
                il.Emit(OpCodes.Call, internalUpdateAllMenus);
                il.Emit(OpCodes.Call, instanceProp.GetMethod);
                il.Emit(OpCodes.Callvirt, rebuildShortcutsMethod);
                il.Emit(OpCodes.Ret);

                _Update = method.CreateDelegate(typeof(Action)) as Action;
            }
        }

        private static void Ldarg(ILGenerator il, int i)
        {
            switch(i)
            {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (i < sbyte.MaxValue)
                        il.Emit(OpCodes.Ldarg_S, i);
                    else
                        il.Emit(OpCodes.Ldarg, i);
                    break;
            }
        }
    }

    internal sealed class MenuList
    {
        private readonly Dictionary<string, (int Priorty, Action Execute, Func<bool> Validate)> pathes = new();

        public void Add(string path, int priority, Action execute, Func<bool> validate = null, bool skipUpdate = false)
        {
            if (!pathes.TryAdd(path, (priority, execute, validate)))
                return;
            MenuHelper.AddMenuItem(path, null, false, priority, execute, validate);
            if (!skipUpdate)
                MenuHelper.Update();
        }

        public void Remove(string path)
        {
            if (!pathes.Remove(path))
                return;

            MenuHelper.RemoveMenuItem(path);
            foreach(var x in pathes)
            {
                MenuHelper.RemoveMenuItem(x.Key);
                if (x.Value.Execute == null)
                    MenuHelper.AddSeparator(x.Key, x.Value.Priorty);
                else
                    MenuHelper.AddMenuItem(x.Key, null, false, x.Value.Priorty, x.Value.Execute, x.Value.Validate);
            }

            MenuHelper.Update();
        }

        public void AddSeparator(string path, int priority)
        {
            if (!pathes.TryAdd(path, (priority, null, null)))
                return;
            MenuHelper.AddSeparator(path, priority);
            MenuHelper.Update();
        }
    }
}
