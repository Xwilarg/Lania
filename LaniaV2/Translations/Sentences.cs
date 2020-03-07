using DiscordUtils;

namespace LaniaV2.Translations
{
    public class Sentences
    {
        private static string Translate(ulong? guildId, string key, params string[] args)
               => Utils.Translate(Program.P.Translations, guildId != null ? Program.P.LaniaDb.Languages[guildId.Value] : null, key, args);
    }
}
