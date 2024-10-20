using System.Runtime.CompilerServices;
using nadena.dev.ndmf.preview;
using UnityEditorInternal;

namespace io.github.azukimochi;

internal static class Preferences
{
    internal const string PathRoot = "LightLimitChanger/";

    public static GlobalPrefs Global => GlobalPrefs.instance;

    public static LocalPrefs Local => LocalPrefs.instance;
}