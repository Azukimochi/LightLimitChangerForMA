using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor;

namespace io.github.azukimochi
{
    internal sealed class LangVersionModifier : AssetPostprocessor
    {
        public static string OnGeneratedCSProject(string path, string content)
        {
            if (!path.Contains("io.github.azukimochi.light-limit-changer", StringComparison.OrdinalIgnoreCase))
                return content;

            var xml = XDocument.Parse(content);
            var langVersion = xml.Root.Elements("PropertyGroup")
                .SelectMany(x => x.Elements("LangVersion"))
                .FirstOrDefault();
            if (langVersion is not null)
            {
                langVersion.Value = "10";
            }
            else
            {
                xml.Root.Element("PropertyGroup").Add(new XElement("LangVersion", "10"));
            }

            return xml.ToString();
        }
    }
}
