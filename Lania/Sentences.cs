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

        public readonly static string hiStr = "Hi.";
        public readonly static string help = "You can use the commands 'Gate open' and 'Gate close' to manage it." + Environment.NewLine +
                                            "You will receive images from the game, you can react to them using reactions." + Environment.NewLine +
                                            "You can also send images by posting them on the channel where the gate is open, and see the reactions that people add on them." + Environment.NewLine +
                                            "Then you can use the command 'Gate stats' to see what reactions you received the most" + Environment.NewLine +
                                            "You can also use 'Gate status' to see the current status of the gate.";
        public static string OnlyUser(string user) { return ("Only " + user + " can do this command.");  }
        public readonly static string noEmote = "You didn't receive any emote yet.";
        public readonly static string myEmotes = "Here are the 10 emotes your received the most:";
        public readonly static string gateOpened = "The gate is open.";
        public readonly static string gateClosed = "The gate is close.";
        public readonly static string noGate = "The gate isn't open.";
        public readonly static string nsfwImage = "This image was detected as NSFW by our filter, consequently it wasn't send.";
        public readonly static string noReport = "There isn't any image to report.";
        public readonly static string reportDone = "The last image you received was reported.";
        public readonly static string dontExist = "I didn't find any guild with this id.";
        public readonly static string alreadyBanned = "This is was already ban.";
        public readonly static string banned = "This id is now banned.";
        public readonly static string gateClosedBan = " gates were closed.";
        public readonly static string isBanned = "You can't use this command since you were banned.";
        public readonly static string isBannedImage = "You can't send images since you were banned.";
        public readonly static string userBanned = "You were banned from using the gate because one of your image was considered as not safe for work.";
        public static string WaitImage(string duration) { return ("You must wait at least " + duration + " before sending another image."); }
        public static string GateChannel(string channelName) { return ("The gate is open in " + channelName + "."); }
        public readonly static string noGateHere = "There is no gate open in this guild.";
        public static string NbGates(string nb) { return ("There are a total of " + nb + " gates opened."); }

        public readonly static string uptime = "Uptime";
        public readonly static string latestVersion = "Latest version";
    }
}