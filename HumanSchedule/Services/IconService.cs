using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace PopSimProto.Services
{
    public static class IconService
    {
        public static List<string> CitizenIcons { get; private set; } = new();
        public static List<string> LocationIcons { get; private set; } = new();

        public static void LoadIcons(string citizenFile, string locationFile)
        {
            if (File.Exists(citizenFile))
                CitizenIcons = ParseTextElements(File.ReadAllText(citizenFile, Encoding.UTF8));

            if (File.Exists(locationFile))
                LocationIcons = ParseTextElements(File.ReadAllText(locationFile, Encoding.UTF8));
        }

        /// <summary>
        /// Parst einen String in einzelne Text-Elemente (grapheme clusters),
        /// damit Emojis mit Surrogate Pairs und Variation Selectors korrekt bleiben.
        /// </summary>
        private static List<string> ParseTextElements(string input)
        {
            var result = new List<string>();
            var enumerator = StringInfo.GetTextElementEnumerator(input);
            while (enumerator.MoveNext())
            {
                string element = enumerator.GetTextElement();
                if (!string.IsNullOrWhiteSpace(element))
                    result.Add(element);
            }
            return result.Distinct().ToList();
        }
    }
}
