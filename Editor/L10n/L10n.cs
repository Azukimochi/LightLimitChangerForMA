using System.Linq;
using nadena.dev.ndmf.localization;

namespace io.github.azukimochi;

internal abstract partial class L10n
{
    private const string PreferenceKey = "io.github.azukimochi.light-limit-changer.lang";

    private static readonly L10n[] Instances = { 
        new En_US(), 
        new Ja_JP(),
        new Zh_Hant(),
        new Zh_Hans(),
    };

    public static L10n Default => Instances[0];

    public static L10n Current => Instances[CurrentLanguageIndex];

    public static Localizer NDMFLocalizer = new(Instances[0].Code, () => Instances.Select<L10n, (string, Func<string, string>)>(x => (x.Code, key => key /* TODO: あとでやる！*/)).ToList());

    public static int CurrentLanguageIndex
    {
        get => cachedLanguageIndex;
        set {
            value = Math.Clamp(value, 0, Instances.Length - 1);
            if (cachedLanguageIndex == value)
                return;
            
            EditorPrefs.SetInt(PreferenceKey, value);
            cachedLanguageIndex = value;
        }
    }

    private static int cachedLanguageIndex;

    static L10n()
    {
        EditorApplication.delayCall += () => cachedLanguageIndex = EditorPrefs.GetInt(PreferenceKey, 0);
    }

    protected L10n() { }
}
