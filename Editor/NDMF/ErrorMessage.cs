using System;
using System.Collections.Generic;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;
using UnityEngine;

namespace io.github.azukimochi

{
    public class ErrorMessage : SimpleError
    {
        public ErrorMessage(string titleKey, ErrorSeverity severity)
        {

            TitleKey = titleKey;
            Severity = severity;
            Localizer = new Localizer("en-US", () => new List<(string, Func<string, string>)>
            {
                ("en-US", key => Localization.S(key, 0)),
                ("ja-JP", key => Localization.S(key, 1)),
                ("zh-Hant", key => Localization.S(key, 2)),
                ("zh-Hans", key => Localization.S(key, 3)),
                ("ko-KR", key => Localization.S(key, 4))
            });
        }
        public ErrorMessage(string titleKey, string hintKey, ErrorSeverity severity)
        {
            TitleKey = titleKey;
            Severity = severity;
            HintKey = hintKey;
            Localizer = new Localizer("en-US", () => new List<(string, Func<string, string>)>
            {
                ("en-US", key => Localization.S(key, 0)),
                ("ja-JP", key => Localization.S(key, 1)),
                ("zh-Hant", key => Localization.S(key, 2)),
                ("zh-Hans", key => Localization.S(key, 3)),
                ("ko-KR", key => Localization.S(key, 4))
            });
        }
        public override Localizer Localizer { get; }
        public override string TitleKey { get; }
        public override ErrorSeverity Severity { get; }
        public override string HintKey { get; }
    }
}