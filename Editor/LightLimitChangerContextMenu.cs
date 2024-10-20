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
    private const int MenuPriority = -1000;

    private const string SetupMenuPath = MenuRoot + "Setup";

    static LightLimitChangerContextMenu()
    {
        SceneHierarchyHooks.addItemsToGameObjectContextMenu += (menu, gameObject) =>
        {
            if (!gameObject.TryGetComponent<VRCAvatarDescriptor>(out _))
                return;

            // TODO: 保存済みのプリセットを追加できるようにする
        };

        return;
    }

    [MenuItem(SetupMenuPath, false, MenuPriority)]
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

    [MenuItem(SetupMenuPath, true, MenuPriority)]
    public static bool SetupValidate()
    {
        return Selection.gameObjects.Any(x => x.TryGetComponent<VRCAvatarDescriptor>(out _));
    }
}
