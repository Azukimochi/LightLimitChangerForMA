using System.Linq;
using CustomLocalization4EditorExtension;
using nadena.dev.ndmf.localization;

namespace io.github.azukimochi;

internal static class L10n
{
    private const string PreferenceKey = "io.github.azukimochi.light-limit-changer.lang";

    [AssemblyCL4EELocalization]
    public static Localization Localization { get; } = new Localization("e955a6e9f59e118418cedcf05a7d1a4e", "ja", PreferenceKey);

    private static GUIContent tempContent;

    public static GUIContent Tr(string localizationKey)
    {
        var text = Localization.Tr(localizationKey);
        if (tempContent == null)
        {
            tempContent = new(text);
        }
        else
        {
            tempContent.text = text;
        }
        return tempContent;
    }

    public static string TrStr(string localizationKey) 
        => Localization.Tr(localizationKey);

    public static Localizer Localizer { get; } = 
        new Localizer("ja", () => Localization.LocalizationByIsoCode.Select(x => ValueTuple.Create<string, Func<string, string>>(x.Key, y => x.Value.TryGetLocalizedString(y))).ToList());
}
