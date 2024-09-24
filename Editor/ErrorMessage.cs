using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;

namespace io.github.azukimochi;

internal class ErrorMessage : SimpleError
{
    public ErrorMessage(string titleKey, ErrorSeverity severity)
    {
        TitleKey = titleKey;
        Severity = severity;
    }

    public override Localizer Localizer => L10n.NDMFLocalizer;

    public override string TitleKey { get; }

    public override ErrorSeverity Severity { get; }

    public static void Throw(string titleKey, ErrorSeverity severity = ErrorSeverity.NonFatal) 
        => ErrorReport.ReportError(new ErrorMessage(titleKey, severity));
}