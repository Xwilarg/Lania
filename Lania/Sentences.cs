﻿using System;

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
        public readonly static string help = "My role is to create an image gate between guilds." + Environment.NewLine +
                                            "If it's your first time using this feature, go in a channel and do @Lania#2961 Gate open" + Environment.NewLine + Environment.NewLine +
                                            "Once a gate is open in a channel, you can send an image and see the reactions people add to it." + Environment.NewLine +
                                            "If you receive an image, you can also add reactions to it, people who send it will then see them." + Environment.NewLine + Environment.NewLine +
                                            "If you open the gate in a SFW channel you must send only SFW content (that mean no picture containing nudity, violence, or simply no picture against Discord ToS)." + Environment.NewLine +
                                            "The only type of NSFW content allowed in NSFW channels is nudity (as long as it follow Discord ToS)" + Environment.NewLine +
                                            "Any attempt to bypass our filter will result in a permanent ban." + Environment.NewLine +
                                            "Please make not that SFW channels can send images to both NSFW and SFW channels but NSFW channels can only send images to others NSFW channels." + Environment.NewLine + Environment.NewLine +
                                            "You can use the following commands to interact with the gate:" + Environment.NewLine +
                                            "**Gate open:** Open the image gate" + Environment.NewLine +
                                            "**Gate close:** Close the image gate" + Environment.NewLine +
                                            "**Gate status:** Display where the gate is opened and how many there are" + Environment.NewLine +
                                            "**Gate stats:** Display how many emotes your received" + Environment.NewLine +
                                            "**Gate report:** Report the last image you received as inappropriate" + Environment.NewLine +
                                            "**Infos:** Display various informations about the bot" + Environment.NewLine +
                                            "**Help:** Display this help";
        public static string OnlyUser(string user) { return ("Only " + user + " can do this command.");  }
        public readonly static string noEmote = "You didn't receive any emote yet.";
        public readonly static string myEmotes = "Here are the 10 emotes your received the most:";
        public readonly static string gateOpened = "The gate is open.";
        public readonly static string gateClosed = "The gate is close.";
        public readonly static string noGate = "The gate isn't open.";
        public readonly static string nsfwImage = "This image was detected as NSFW by our filter, consequently it wasn't send.";
        public readonly static string wrongNsfw = "Please make note that we only allow nudity in NSFW channels.";
        public readonly static string noReport = "There isn't any image to report.";
        public readonly static string reportDone = "The last image you received was reported.";
        public readonly static string dontExist = "I didn't find any guild with this id.";
        public readonly static string alreadyBanned = "This is was already ban.";
        public readonly static string banned = "This id is now banned.";
        public readonly static string gateClosedBan = " gates were closed.";
        public readonly static string isBanned = "You can't use this command since you were banned.";
        public readonly static string isBannedImage = "You can't send images since you were banned.";
        public readonly static string userBanned = "You were banned from using the gate because of the following reason: ";
        public readonly static string noChan = "There is no channel available to receive your image.";
        public readonly static string waitMsg = "Please wait...";
        public static string WaitImage(string duration) { return ("You must wait at least " + duration + " before sending another image."); }
        public static string GateChannel(string channelName) { return ("The gate is open in " + channelName + "."); }
        public readonly static string noGateHere = "There is no gate open in this guild.";
        public static string NbGates(string nb, string relativeNb, string readNb) { return ("There are a total of " + nb + " gates opened." + Environment.NewLine +
                                                                                            "You can receive images from " + readNb + " of them." + Environment.NewLine +
                                                                                            "You can send images in " + relativeNb + " of them."); }

        public readonly static string uptime = "Uptime";
        public readonly static string latestVersion = "Latest version";
        public readonly static string author = "Author";
    }
}