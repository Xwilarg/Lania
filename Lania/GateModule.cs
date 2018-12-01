using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lania
{
    [Group("Gate")]
    public class GateModule : ModuleBase
    {
        [Command("Open", RunMode = RunMode.Async), Summary("Open the image gate")]
        public async Task OpenGate()
        {
            if (Context.Guild.OwnerId != Context.User.Id)
                await ReplyAsync(Sentences.OnlyUser(Context.Guild.Id, (await Context.Guild.GetOwnerAsync()).ToString()));
            else if (await Program.p.GetDb().IsBan(Context.User.Id.ToString()))
                await ReplyAsync(Sentences.IsBanned(Context.Guild.Id));
            else
            {
                await Program.p.GetDb().OpenGate(Context.Guild.Id, Context.Channel.Id);
                await ReplyAsync(Sentences.GateOpened(Context.Guild.Id));
            }
        }

        [Command("Close", RunMode = RunMode.Async), Summary("Close the image gate")]
        public async Task CloseGate()
        {
            if (Context.Guild.OwnerId != Context.User.Id)
                await ReplyAsync(Sentences.OnlyUser(Context.Guild.Id, (await Context.Guild.GetOwnerAsync()).ToString()));
            else if (await Close(Context.Guild.Id))
                await ReplyAsync(Sentences.GateClosed(Context.Guild.Id));
            else
                await ReplyAsync(Sentences.NoGate(Context.Guild.Id));
        }

        [Command("Stats"), Summary("Stats about emotes received")]
        public async Task StatsGate()
        {
            IEnumerable<dynamic> emotes = await Program.p.GetDb().GetEmotes(Context.Guild.Id);
            if (emotes != null)
            {
                Dictionary<string, int> allEmotes = new Dictionary<string, int>();
                foreach (var f in emotes)
                {
                    Match match = Regex.Match(f.ToString(), "\"([^\"]+)\": \"([0-9]+)\"");
                    if (match.Groups[1].Value != "id")
                        allEmotes.Add(match.Groups[1].Value, int.Parse(match.Groups[2].Value));
                }
                if (allEmotes.Count == 0)
                    await ReplyAsync(Sentences.NoEmote(Context.Guild.Id));
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
                    await ReplyAsync(Sentences.MyEmotes(Context.Guild.Id) + Environment.NewLine + finalStr);
                }
            }
            else
                await ReplyAsync(Sentences.NoEmote(Context.Guild.Id));
        }

        [Command("Status"), Summary("Get informations about the gate")]
        public async Task StatusGate()
        {
            string finalStr = "";
            string id = await Program.p.GetDb().GetGateChan(Context.Guild.Id);
            if (id != null)
                finalStr += Sentences.GateChannel(Context.Guild.Id, "<#" + id + ">");
            else
                finalStr += Sentences.NoGateHere(Context.Guild.Id);
            int total, relative, read;
            relative = Program.p.GetDb().GetAllGuilds(Context.Guild.Id, (Context.Channel as ITextChannel).IsNsfw, out total, out read).Count;
            await ReplyAsync(finalStr + Environment.NewLine + Sentences.NbGates(Context.Guild.Id, total.ToString(), relative.ToString(), read.ToString()));
        }

        [Command("Report", RunMode = RunMode.Async), Summary("Report the last image")]
        public async Task Report()
        {
            string last = await Program.p.GetDb().GetLast(Context.Guild.Id);
            if (await Program.p.GetDb().IsBan(Context.User.Id.ToString()))
                await ReplyAsync(Sentences.IsBanned(Context.Guild.Id));
            else if (last != null)
            {
                string[] content = last.Split('|');
                await Program.p.GetDb().DeleteLast(Context.Guild.Id);
                await Program.p.client.GetGuild(Sentences.refGuild).GetTextChannel(Sentences.refChannel).SendMessageAsync(
                    "", false, new EmbedBuilder
                    {
                        Color = Color.Red,
                        Title = "Report of user " + content[0] + " by " + Context.User.Id + ", chan NSFW: " + content[4],
                        Description = "<" + content[1] + "> the " + DateTime.Now.ToString("dd/MM/yy HH:mm:ss")
                    }.Build());
                await ((await (await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(content[2]))).GetMessageAsync(Convert.ToUInt64(content[3]))) as IUserMessage).ModifyAsync(
                    x => x.Embed = new EmbedBuilder { Color = Color.Red, Title = Sentences.Reported(Context.Guild.Id) }.Build());
                await ReplyAsync(Sentences.ReportDone(Context.Guild.Id));
            }
            else
                await ReplyAsync(Sentences.NoReport(Context.Guild.Id));
        }

        [Command("Ban"), Summary("Ban an user from using the gate")]
        public async Task Ban(string id, params string[] reason)
        {
            if (Context.User.Id != Sentences.ownerId)
            {
                await ReplyAsync(Sentences.OnlyUser(Context.Guild.Id, Sentences.ownerName));
                return;
            }
            bool banned = false;
            string finalReason = string.Join(" ", reason);
            if (await Program.p.GetDb().IsBan(id))
                await ReplyAsync(Sentences.AlreadyBanned(Context.Guild.Id));
            else
            {
                await Program.p.GetDb().Ban(id, finalReason);
                await ReplyAsync(Sentences.Banned(Context.Guild.Id));
                banned = true;
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
                        await user.SendMessageAsync(Sentences.UserBanned(Context.Guild.Id) + finalReason);
                        wasPm = true;
                    }
                }
                if (sg.OwnerId.ToString() == id)
                {
                    i++;
                    await Close(sg.Id);
                }
            }
            if (i > 0)
                await ReplyAsync(i + Sentences.GateClosedBan(Context.Guild.Id));
        }

        public static async Task<bool> Close(ulong guildId)
        {
            return (await Program.p.GetDb().CloseGate(guildId));
        }
    }
}