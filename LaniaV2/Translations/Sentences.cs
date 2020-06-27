using DiscordUtils;

namespace LaniaV2.Translations
{
    public class Sentences
    {
        private static string Translate(ulong? guildId, string key, params string[] args)
               => Utils.Translate(Program.P.Translations, guildId != null ? Program.P.Manager.GetLanguage(guildId.Value) : null, key, args);

        // Open / Close
        public static string OnlyManage(ulong? guildId) { return Translate(guildId, "onlyManage"); }
        public static string IsBanned(ulong? guildId) { return Translate(guildId, "isBanned"); }
        public static string GateAlredyOpened(ulong? guildId) { return Translate(guildId, "gateAlredyOpened"); }
        public static string TooManyGatesOpened(ulong? guildId, int max) { return Translate(guildId, "tooManyGatesOpened", max.ToString()); }
        public static string NoGate(ulong? guildId) { return Translate(guildId, "noGate"); }
        public static string GateOpened(ulong? guildId) { return Translate(guildId, "gateOpened"); }
        public static string GateClosed(ulong? guildId) { return Translate(guildId, "gateClosed"); }

        // Status
        public static string NoGateHere(ulong? guildId) { return Translate(guildId, "noGateHere"); }
        public static string GateOpenedIn(ulong? guildId, string chan, bool isNsfw) { return Translate(guildId, "gateOpenedIn", chan, isNsfw ? "NSFW" : "SFW"); }

        // Ban
        public static string OnlyUser(ulong? guildId) { return Translate(guildId, "onlyUser"); }
        public static string NotUlong(ulong? guildId) { return Translate(guildId, "notUlong"); }
        public static string AlreadyBanned(ulong? guildId) { return Translate(guildId, "alreadyBanned"); }
        public static string Banned(ulong? guildId) { return Translate(guildId, "banned"); }
        public static string UserBanned(ulong? guildId, string reason, int gatesClosed) { return Translate(guildId, "userBanned", reason, gatesClosed.ToString()); }
        public static string GateClosedBan(ulong? guildId, int nbGates) { return Translate(guildId, "gateClosedBan", nbGates.ToString()); }
    }
}
