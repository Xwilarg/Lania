using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Lania
{
    public static class Translation
    {
        private readonly static string translationFolder = "../../Lania-translations/Translations";

        public static string Translate(ulong guildId, string word, params string[] args)
        {
            var translations = Program.p.GetTranslations();
            string wordLanguage = Program.p.GetLanguages()[guildId];
            string sentence;
            if (translations[wordLanguage].ContainsKey(word))
                sentence = translations[wordLanguage][word];
            sentence = translations["en"][word];
            sentence = sentence.Replace("\\n", "\n");
            for (int i = 0; i < args.Length; i++)
                sentence = sentence.Replace("{" + i + "}", args[i]);
            return (sentence);
        }

        public static void Init(Dictionary<string, Dictionary<string, string>> translations,
            Dictionary<string, List<string>> translationKeyAlternate)
        {
            if (!Directory.Exists("Translations"))
                Directory.CreateDirectory("Translations");
            if (Directory.Exists(translationFolder))
            {
                foreach (string dir in Directory.GetDirectories(translationFolder))
                {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        FileInfo fi = new FileInfo(file);
                        File.Copy(file, "Translations/" + di.Name + "-" + fi.Name, true);
                    }
                }
            }
            foreach (string file in Directory.GetFiles("Translations"))
            {
                FileInfo fi = new FileInfo(file);
                Match match = Regex.Match(fi.Name, "([a-z]+)-(infos|terms).json");
                if (match.Groups.Count < 3)
                    continue;
                string key = match.Groups[1].Value;
                if (match.Groups[2].Value == "infos")
                {
                    dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(file));
                    translationKeyAlternate.Add(key, new List<string>()
                    {
                        json.nameEnglish.ToString(),
                        json.nameLanguage.ToString()
                    });
                }
                else
                {
                    translations.Add(key, new Dictionary<string, string>());
                    foreach (Match m in Regex.Matches(File.ReadAllText(file), "\"([a-zA-Z0-9]+)\" ?: ?\"([^\"]+)\""))
                        translations[key].Add(m.Groups[1].Value, m.Groups[2].Value);
                }
            }
        }
    }
}
