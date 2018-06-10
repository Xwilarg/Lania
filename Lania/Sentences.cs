using System;

namespace Lania
{
    public static class Sentences
    {
        public readonly static ulong myId = 454742499085254656;

        public readonly static string hiStr = "Hi.";
        public readonly static string help = "You can use the commands 'Open gate' and 'Close gate' to manage it." + Environment.NewLine +
                                            "You will receive images from the game, you can react to them using reactions." + Environment.NewLine +
                                            "You can also send images by posting them on the channel where the gate is open, and see the reactions that people add on them.";
        public static string onlyUser(string user) { return ("Only " + user + " can do this command.");  }
        public readonly static string gateOpened = "The gate is open.";
        public readonly static string gateClosed = "The gate is closed.";
        public readonly static string noGate = "The gate isn't open.";
    }
}