namespace io.github.azukimochi;

using VRC.SDK3.Avatars.ScriptableObjects;
using MenuItem = nadena.dev.modular_avatar.core.ModularAvatarMenuItem;

internal static class MenuItemExt
{
    public static MenuItem GetOrAdd(this MenuItem menu, string path, Func<MenuItem, (VRCExpressionsMenu.Control.ControlType ControlType, string ParameterName)> factory)
    {
        var split = path.Split("/");
        bool flag = false;
        for (int i = 0; i < split.Length; i++)
        {
            var name = split[i];
            var child = menu.transform.Find(name);
            if (child == null)
            {
                var obj = new GameObject(name);
                obj.transform.parent = menu.transform;
                child = obj.transform;
            }

            menu = child.GetComponent<MenuItem>();
            flag = menu == null;
            if (flag)
            {
                menu = child.gameObject.AddComponent<MenuItem>();
                menu.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                menu.MenuSource = nadena.dev.modular_avatar.core.SubmenuSource.Children;
            }
        }

        if (!flag)
            return menu;

        var (type, param) = factory(menu);
        menu.Control.type = type;
        var p = new VRCExpressionsMenu.Control.Parameter() { name = param };
        if (type == VRCExpressionsMenu.Control.ControlType.RadialPuppet)
        {
            menu.Control.subParameters = new[] { p };
        }
        else
        {
            menu.Control.parameter = p;
        }

        return menu;
    }
}