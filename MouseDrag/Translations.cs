using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MouseDrag
{
    public static class Translations
    {
        public static Dictionary<string, string> translationDict = null;
        public const string filenameExtension = ".txt";
        public const char translationSeparatorChar = '|';


        public static string[] GetAvailableLanguagePaths()
        {
            string[] ret = new string[0];
            try {
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                assemblyFolder = assemblyFolder.Substring(0, assemblyFolder.LastIndexOf(Path.DirectorySeparatorChar));
                assemblyFolder += Path.DirectorySeparatorChar + "translations";
                ret = Directory.GetFiles(assemblyFolder, "*" + filenameExtension);
            } catch (Exception ex) {
                Plugin.Logger.LogError("Translations.GetAvailableLanguages exception: " + ex?.ToString());
            }
            return ret;
        }


        public static string[] GetLanguagesFromPaths(string[] paths)
        {
            if (paths == null)
                return new string[0];
            string[] ret = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++) {
                string name = Path.GetFileName(paths[i]);
                name = name.Substring(0, name.LastIndexOf('.'));
                ret[i] = name;
            }
            return ret;
        }


        public static void LoadLanguage(RainWorld rainWorld)
        {
            string language = null;
            if (!String.IsNullOrEmpty(Options.language?.Value)) {
                language = Options.language.Value;
            } else {
                language = rainWorld?.inGameTranslator?.currentLanguage?.ToString();
            }
            if (!String.IsNullOrEmpty(language)) {
                LoadLanguage(language);
            } else {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("Translations.LoadLanguage, no valid language was selected");
            }
        }


        public static void LoadLanguage(string language)
        {
            var paths = GetAvailableLanguagePaths();
            var languages = GetLanguagesFromPaths(paths);
            if (String.IsNullOrEmpty(language) || !languages.Any(language.Contains)) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogWarning("Translations.LoadLanguage, no valid language file could be loaded for language \"" + language + "\"");
                return;
            }
            string filename = language + filenameExtension;
            string path = paths.FirstOrDefault(x => x.EndsWith(filename));
            string[] file = null;
            try {
                file = File.ReadAllLines(path);
            } catch (Exception ex) {
                Plugin.Logger.LogError("Translations.LoadLanguage, exception while reading language file: " + ex?.ToString());
                return;
            }
            translationDict = new Dictionary<string, string>();
            for (int i = 0; i < file?.Length; i++) {
                var line = file[i];
                var parts = line?.Split(translationSeparatorChar);
                if (parts?.Length != 2 || parts[0] == null || parts[1] == null) {
                    if (Options.logDebug?.Value != false)
                        Plugin.Logger.LogWarning("Translations.LoadLanguage, skip invalid line in translation file: " + line);
                    continue;
                }
                translationDict.Add(parts[0], parts[1]);
            }
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("Translations.LoadLanguage, language loaded: " + language);
        }


        public static string Translate(string text)
        {
            if (translationDict == null || String.IsNullOrEmpty(text))
                return text;
            if (translationDict.TryGetValue(text, out string translation))
                return translation;
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("Translations.Translate, no translation for text \"" + text + "\"");
            return text;
        }
    }
}
