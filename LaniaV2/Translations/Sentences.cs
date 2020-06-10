using DiscordUtils;

namespace LaniaV2.Translations
{
    public class Sentences
    {
        private static string Translate(ulong? guildId, string key, params string[] args)
               => Utils.Translate(Program.P.Translations, guildId != null ? Program.P.Manager.GetLanguage(guildId.Value) : null, key, args);

        public static string OnlyManage(ulong? guildId) { return Translate(guildId, "onlyManage"); }
        public static string IsBanned(ulong? guildId) { return Translate(guildId, "isBanned"); }
        public static string GateOpened(ulong? guildId) { return Translate(guildId, "gateOpened"); }
        public static string GateAlredyOpened(ulong? guildId) { return Translate(guildId, "gateAlredyOpened"); }
        public static string TooManyGatesOpened(ulong? guildId, int max) { return Translate(guildId, "tooManyGatesOpened", max.ToString()); }
    }
}
