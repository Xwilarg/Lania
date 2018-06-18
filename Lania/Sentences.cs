using System;

namespace Lania
{
    public static class Sentences
    {
        public readonly static ulong myId = 454742499085254656;

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
        public static string GateChannel(string channelName) { return ("The gate is open in " + channelName + "."); }
        public readonly static string noGateHere = "There is no gate open in this guild.";
        public static string NbGates(string nb) { return ("There are a total of " + nb + " gates opened."); }

        public readonly static string uptime = "Uptime";
        public readonly static string latestVersion = "Latest version";
    }
}