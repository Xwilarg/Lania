﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lania
{
    [Group("Gate")]
    public class GateModule : ModuleBase
    {
        [Command("Open"), Summary("Open the image gate")]
        public async Task OpenGate()
        {
            if (Context.Guild.OwnerId != Context.User.Id)
                await ReplyAsync(Sentences.OnlyUser((await Context.Guild.GetOwnerAsync()).ToString()));
            else if (IsBanned(Context.User.Id))
                await ReplyAsync(Sentences.isBanned);
            else
            {
                if (!Directory.Exists("Saves/Guilds"))
                    Directory.CreateDirectory("Saves/Guilds");
                File.WriteAllText("Saves/Guilds/" + Context.Guild.Id + ".dat", Context.Channel.Id.ToString());
                await ReplyAsync(Sentences.gateOpened);
            }
        }

        [Command("Close"), Summary("Close the image gate")]
        public async Task CloseGate()
        {
            if (Context.Guild.OwnerId != Context.User.Id)
                await ReplyAsync(Sentences.OnlyUser((await Context.Guild.GetOwnerAsync()).ToString()));
            else if (Close(Context.Guild.Id))
                await ReplyAsync(Sentences.gateClosed);
            else
                await ReplyAsync(Sentences.noGate);
        }

        [Command("Stats"), Summary("Stats about emotes received")]
        public async Task StatsGate()
        {
            if (Directory.Exists("Saves/Emotes") && Directory.Exists("Saves/Emotes/" + Context.Guild.Id))
            {
                Dictionary<string, int> allEmotes = new Dictionary<string, int>();
                foreach (string f in Directory.GetFiles("Saves/Emotes/" + Context.Guild.Id))
                {
                    FileInfo fi = new FileInfo(f);
                    int nb = Convert.ToInt32(File.ReadAllText(f));
                    if (nb > 0)
                        allEmotes.Add(fi.Name.Split('.')[0].Replace('-', ':'), nb);
                }
                if (allEmotes.Count == 0)
                    await ReplyAsync(Sentences.noEmote);
                else
                {
                    string finalStr = "";
                    int counter = 10;
                    while (counter > 0 && allEmotes.Count > 0)
                    {
                        KeyValuePair<string, int>? biggest = null;
                        foreach (var kv in allEmotes)
                        {
                            if (biggest == null || kv.Value > biggest.Value.Value)
                                biggest = kv;
                        }
                        finalStr += biggest.Value.Key + " x" + biggest.Value.Value + Environment.NewLine;
                        allEmotes.Remove(biggest.Value.Key);
                        counter--;
                    }
                    await ReplyAsync(Sentences.myEmotes + Environment.NewLine + finalStr);
                }
            }
            else
                await ReplyAsync(Sentences.noEmote);
        }

        [Command("Status"), Summary("Get informations about the gate")]
        public async Task StatusGate()
        {
            string finalStr = "";
            if (File.Exists("Saves/Guilds/" + Context.Guild.Id + ".dat"))
                finalStr += Sentences.GateChannel("<#" + File.ReadAllText("Saves/Guilds/" + Context.Guild.Id + ".dat") + ">");
            else
                finalStr += Sentences.noGateHere;
            int total, relative, read;
            relative = Program.GetNbChans(Context.Guild.Id, (Context.Channel as ITextChannel).IsNsfw, out total, out read).Count;
            await ReplyAsync(finalStr + Environment.NewLine + Sentences.NbGates(total.ToString(), relative.ToString(), read.ToString()));
        }

        [Command("Report", RunMode = RunMode.Async), Summary("Report the last image")]
        public async Task Report()
        {
            if (IsBanned(Context.User.Id))
                await ReplyAsync(Sentences.isBanned);
            else if (Directory.Exists("Saves/Guilds/" + Context.Guild.Id) && File.Exists("Saves/Guilds/" + Context.Guild.Id + "/last.dat"))
            {
                string[] content = File.ReadAllLines("Saves/Guilds/" + Context.Guild.Id + "/last.dat");
                File.Delete("Saves/Guilds/" + Context.Guild.Id + "/last.dat");
                await Program.p.client.GetGuild(Sentences.refGuild).GetTextChannel(Sentences.refChannel).SendMessageAsync(
                    "", false, new EmbedBuilder
                    {
                        Color = Color.Red,
                        Title = "Report of user " + content[0] + " by " + Context.User.Id + ", chan NSFW: " + content[4],
                        Description = "<" + content[1] + "> the " + DateTime.Now.ToString("dd/MM/yy HH:mm:ss")
                    }.Build());
                await ((await (await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(content[2]))).GetMessageAsync(Convert.ToUInt64(content[3]))) as IUserMessage).ModifyAsync(
                    x => x.Embed = new EmbedBuilder { Color = Color.Red, Title = "This message was reported" }.Build());
                await ReplyAsync(Sentences.reportDone);
            }
            else
                await ReplyAsync(Sentences.noReport);
        }

        [Command("Ban"), Summary("Ban an user from using the gate")]
        public async Task Ban(string id, string reason)
        {
            if (Context.User.Id != Sentences.ownerId)
            {
                await ReplyAsync(Sentences.OnlyUser(Sentences.ownerName));
                return;
            }
            bool banned = false;
            if (File.Exists("Saves/ban.dat"))
            {
                if (File.ReadAllLines("Saves/ban.dat").Contains(id))
                    await ReplyAsync(Sentences.alreadyBanned);
                else
                {
                    banned = true;
                    File.AppendAllText("Saves/ban.dat", Environment.NewLine + id);
                    await ReplyAsync(Sentences.banned);
                }
            }
            else
            {
                banned = true;
                File.WriteAllText("Saves/ban.dat", id);
                await ReplyAsync(Sentences.banned);
            }
            int i = 0;
            bool wasPm = false;
            foreach (SocketGuild sg in Program.p.client.Guilds)
            {
                if (banned && !wasPm)
                {
                    SocketGuildUser user = sg.Users.ToList().Find(x => x.Id.ToString() == id);
                    if (user != null)
                    {
                        await user.SendMessageAsync(Sentences.userBanned + reason);
                        wasPm = true;
                    }
                }
                if (sg.OwnerId.ToString() == id)
                {
                    i++;
                    Close(sg.Id);
                }
            }
            if (i > 0)
                await ReplyAsync(i + Sentences.gateClosedBan);
        }

        public static bool Close(ulong guildId)
        {
            if (File.Exists("Saves/Guilds/" + guildId + ".dat"))
            {
                File.Delete("Saves/Guilds/" + guildId + ".dat");
                return (true);
            }
            return (false);
        }

        public static bool IsBanned(ulong guildId)
        {
            return (File.Exists("Saves/ban.dat") && File.ReadAllLines("Saves/ban.dat").Contains(guildId.ToString()));
        }
    }
}