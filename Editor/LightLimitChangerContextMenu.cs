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
using UnityEditor.SceneManagement;
using VRC.SDK3.Avatars.Components;

namespace io.github.azukimochi;

[InitializeOnLoad]
internal static class LightLimitChangerContextMenu
{
    private const string MenuRoot = "GameObject/" + LightLimitChanger.Title + "/";
    private const int MenuPriority = -100;

    private const string SetupMenuPath = MenuRoot + "Setup";

    static LightLimitChangerContextMenu()
    {
        const string PathRoot = LightLimitChanger.Title + "/" + "Preset/";
        SceneHierarchyHooks.addItemsToGameObjectContextMenu += (menu, gameObject) =>
        {
            if (!Selection.gameObjects.Any(x => x.TryGetComponent<VRCAvatarDescriptor>(out _)))
                return;

            // TODO: 保存済みのプリセットを追加できるようにする

            foreach(var key in PresetManager.Local.Keys)
            {
                menu.AddItem(new($"{PathRoot}{key} [Local]"), false, static context => Setup(component => PresetManager.Local.TryLoad(context as string, component)), key);
            }

            foreach (var key in PresetManager.Global.Keys)
            {
                menu.AddItem(new($"{PathRoot}{key} [Global]"), false, static context => Setup(component => PresetManager.Global.TryLoad(context as string, component)), key);
            }

            menu.AddSeparator($"{PathRoot}");
            menu.AddItem(new($"{PathRoot}Open Preset Manager ..."), false, () => { });
        };

        return;
    }

    [MenuItem(SetupMenuPath, false, MenuPriority)]
    public static void Setup() => Setup(null);

    [MenuItem(SetupMenuPath, true, MenuPriority)]
    public static bool SetupValidate()
    {
        return Selection.gameObjects.Any(x => x.TryGetComponent<VRCAvatarDescriptor>(out _));
    }

    private static void Setup(Action<LightLimitChangerComponent> factory)
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

            var prefab = PresetManager.DefaultSettings;
            if (prefab != null)
            {
                prefab = PrefabUtility.InstantiatePrefab(prefab, avatarRoot.transform) as GameObject;
                prefab.name = LightLimitChanger.Title;
                if (!prefab.TryGetComponent<LightLimitChangerComponent>(out _))
                    prefab.AddComponent<LightLimitChangerComponent>();
            }
            else
            {
                prefab = new GameObject(LightLimitChanger.Title);
                prefab.AddComponent<LightLimitChangerComponent>();
                prefab.transform.parent = avatarRoot.transform;
            }

            factory?.Invoke(prefab.GetComponent<LightLimitChangerComponent>());

            select = prefab;

            EditorGUIUtility.PingObject(prefab);
        }
        Selection.objects = selection;
    }
}
