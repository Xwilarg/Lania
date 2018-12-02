using System;

namespace Lania
{
    public static class Sentences
    {
        public readonly static ulong myId = Program.p.client.CurrentUser.Id;
        public readonly static ulong refGuild = 146701031227654144;
        public readonly static ulong refChannel = 459341718228172811;
        public readonly static ulong ownerId = 144851584478740481;
        public readonly static string ownerName = "Zirk#0001";
        public readonly static string inviteLink = "https://discordapp.com/api/oauth2/authorize?client_id=454742499085254656&permissions=346112&scope=bot";

        public static string Help(ulong guildId)
        {
            return (Translation.Translate(guildId, "help1") + Environment.NewLine +
                Translation.Translate(guildId, "help2") + Environment.NewLine + Environment.NewLine +
                Translation.Translate(guildId, "help3") + Environment.NewLine +
                Translation.Translate(guildId, "help4") + Environment.NewLine + Environment.NewLine +
                Translation.Translate(guildId, "help5") + Environment.NewLine +
                Translation.Translate(guildId, "help6") + Environment.NewLine +
                Translation.Translate(guildId, "help7") + Environment.NewLine +
                Translation.Translate(guildId, "help8") + Environment.NewLine + Environment.NewLine +
                Translation.Translate(guildId, "help9") + Environment.NewLine +
                Translation.Translate(guildId, "help10") + Environment.NewLine +
                Translation.Translate(guildId, "help11") + Environment.NewLine +
                Translation.Translate(guildId, "help12") + Environment.NewLine +
                Translation.Translate(guildId, "help13") + Environment.NewLine +
                Translation.Translate(guildId, "help14") + Environment.NewLine +
                Translation.Translate(guildId, "help15") + Environment.NewLine +
                Translation.Translate(guildId, "help16") + Environment.NewLine +
                Translation.Translate(guildId, "help17") + Environment.NewLine +
                Translation.Translate(guildId, "help18"));
        }
        public static string OnlyUser(ulong guildId, string user) { return (Translation.Translate(guildId, "onlyUser", user)); }
        public static string NoEmote(ulong guildId) { return (Translation.Translate(guildId, "noEmote")); }
        public static string MyEmotes(ulong guildId) { return (Translation.Translate(guildId, "myEmotes")); }
        public static string GateOpened(ulong guildId) { return (Translation.Translate(guildId, "gateOpened")); }
        public static string GateClosed(ulong guildId) { return (Translation.Translate(guildId, "gateClosed")); }
        public static string NoGate(ulong guildId) { return (Translation.Translate(guildId, "noGate")); }
        public static string NsfwImage(ulong guildId) { return (Translation.Translate(guildId, "nsfwImage")); }
        public static string WrongNsfw(ulong guildId) { return (Translation.Translate(guildId, "wrongNsfw")); }
        public static string NoReport(ulong guildId) { return (Translation.Translate(guildId, "noReport")); }
        public static string ReportDone(ulong guildId) { return (Translation.Translate(guildId, "reportDone")); }
        public static string DontExist(ulong guildId) { return (Translation.Translate(guildId, "dontExist")); }
        public static string AlreadyBanned(ulong guildId) { return (Translation.Translate(guildId, "alreadyBanned")); }
        public static string Banned(ulong guildId) { return (Translation.Translate(guildId, "banned")); }
        public static string GateClosedBan(ulong guildId) { return (Translation.Translate(guildId, "gateClosedBan")); }
        public static string IsBanned(ulong guildId) { return (Translation.Translate(guildId, "isBanned")); }
        public static string IsBannedImage(ulong guildId) { return (Translation.Translate(guildId, "isBannedImage")); }
        public static string UserBanned(ulong guildId) { return (Translation.Translate(guildId, "userBanned")); }
        public static string NoChan(ulong guildId) { return (Translation.Translate(guildId, "noChan")); }
        public static string WaitMsg(ulong guildId) { return (Translation.Translate(guildId, "waitMsg")); }
        public static string Reported(ulong guildId) { return (Translation.Translate(guildId, "reported")); }
        public static string LastImage(ulong guildId) { return (Translation.Translate(guildId, "lastImage")); }
        public static string NothingYet(ulong guildId) { return (Translation.Translate(guildId, "nothingYet")); }
        public static string ImageReceived(ulong guildId) { return (Translation.Translate(guildId, "imageReceived")); }
        public static string EmoteHelp(ulong guildId) { return (Translation.Translate(guildId, "emoteHelp")); }
        public static string WaitImage(ulong guildId, string duration) { return (Translation.Translate(guildId, "waitImage", duration)); }
        public static string GateChannel(ulong guildId, string channelName) { return (Translation.Translate(guildId, "gateChannel", channelName)); }
        public static string NoGateHere(ulong guildId) { return (Translation.Translate(guildId, "noGateHere")); }
        public static string NbGates(ulong guildId, string nb, string relativeNb, string readNb) {
            return (Translation.Translate(guildId, "nbGates1", nb) + Environment.NewLine +
                Translation.Translate(guildId, "nbGates2", relativeNb) + Environment.NewLine +
                Translation.Translate(guildId, "nbGates3", readNb));
        }
        public static string FileSent(ulong guildId, string guildCount) { return (Translation.Translate(guildId, "fileSent", guildCount)); }
        
        public static string Uptime(ulong guildId) { return (Translation.Translate(guildId, "uptime")); }
        public static string LatestVersion(ulong guildId) { return (Translation.Translate(guildId, "latestVersion")); }
        public static string Author(ulong guildId) { return (Translation.Translate(guildId, "author")); }
        public static string InvitationLink(ulong guildId) { return (Translation.Translate(guildId, "invitationLink")); }
        public static string TimeSeconds(ulong guildId, string seconds) { return (Translation.Translate(guildId, "timeSeconds", seconds)); }
        public static string TimeMinutes(ulong guildId, string minutes, string seconds) { return (Translation.Translate(guildId, "timeMinutes", minutes, seconds)); }
        public static string TimeHours(ulong guildId, string hours, string minutes, string seconds) { return (Translation.Translate(guildId, "timeHours", hours, minutes, seconds)); }
        public static string TimeDays(ulong guildId, string days, string hours, string minutes, string seconds) { return (Translation.Translate(guildId, "timeDays", days, hours, minutes, seconds)); }
        public static string LanguageHelp(ulong guildId) { return (Translation.Translate(guildId, "languageHelp")); }
        public static string InvalidLanguage(ulong guildId) { return (Translation.Translate(guildId, "invalidLanguage")); }
        public static string LanguageChanged(ulong guildId) { return (Translation.Translate(guildId, "languageChanged")); }

        public static string DateTimeFormat(ulong guildId) { return (Translation.Translate(guildId, "dateTimeFormat")); }
        public static string Error(ulong guildId, string msg) { return (Translation.Translate(guildId, "error", msg)); }
    }
}