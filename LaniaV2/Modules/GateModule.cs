using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordUtils;
using LaniaV2.Translations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaniaV2.Modules
{
    [Group("Gate")]
    public class GateModule : ModuleBase
    {
        private static bool CanModify(IUser user, ulong ownerId)
        {
            if (user == null)
                return false;
            if (user.Id == ownerId)
                return true;
            var guildUser = (IGuildUser)user;
            return guildUser.GuildPermissions.ManageGuild;
        }

        [Command("Open")]
        public async Task OpenGate(params string[] args)
        {
            if (!CanModify(Context.User, Context.Guild.OwnerId))
                await ReplyAsync(Sentences.OnlyManage(Context.Guild.Id));
            else if (Program.P.Manager.IsBanned(Context.User.Id))
                await ReplyAsync(Sentences.IsBanned(Context.Guild.Id));
            else
            {
                ITextChannel chan;
                if (args.Length > 0)
                {
                    chan = await Utils.GetTextChannel(string.Join(" ", args), Context.Guild);
                    if (chan == null)
                    {
                        await ReplyAsync(Sentences.GateUnknown(Context.Guild.Id));
                        return;
                    }
                }
                else
                    chan = (ITextChannel)Context.Channel;
                Core.Guild guild = Program.P.Manager.GetGuild(Context.Guild.Id);
                if (guild.DoesGateExist(chan.Id))
                    await ReplyAsync(Sentences.GateAlredyOpened(Context.Guild.Id));
                else if (guild.DidReachMaxLimitGate())
                    await ReplyAsync(Sentences.TooManyGatesOpened(Context.Guild.Id, guild.GetMaxLimitGate()));
                else
                {
                    guild.AddGate(chan.Id);
                    await ReplyAsync(Sentences.GateOpened(Context.Guild.Id));
                }
            }
        }

        [Command("Close")]
        public async Task CloseGate(params string[] args)
        {
            if (!CanModify(Context.User, Context.Guild.OwnerId))
                await ReplyAsync(Sentences.OnlyManage(Context.Guild.Id));
            else if (Program.P.Manager.IsBanned(Context.User.Id))
                await ReplyAsync(Sentences.IsBanned(Context.Guild.Id));
            else
            {
                ITextChannel chan;
                if (args.Length > 0)
                {
                    chan = await Utils.GetTextChannel(string.Join(" ", args), Context.Guild);
                    if (chan == null)
                    {
                        await ReplyAsync(Sentences.GateUnknown(Context.Guild.Id));
                        return;
                    }
                }
                else
                    chan = (ITextChannel)Context.Channel;
                Core.Guild guild = Program.P.Manager.GetGuild(Context.Guild.Id);
                if (!guild.DoesGateExist(chan.Id))
                    await ReplyAsync(Sentences.NoGate(Context.Guild.Id));
                else
                {
                    guild.RemoveGate(chan.Id);
                    await ReplyAsync(Sentences.GateClosed(Context.Guild.Id));
                }
            }
        }

        [Command("Status")]
        public async Task StatusGuild(params string[] _)
        {
            if (Program.P.Manager.IsBanned(Context.User.Id))
                await ReplyAsync(Sentences.IsBanned(Context.Guild.Id));
            else
            {
                Core.Guild guild = Program.P.Manager.GetGuild(Context.Guild.Id);
                if (!guild.HaveAnyGate())
                    await ReplyAsync(Sentences.NoGateHere(Context.Guild.Id));
                else
                {
                    StringBuilder str = new StringBuilder();
                    foreach (var gate in guild.GetGates())
                    {
                        ITextChannel chan = await Context.Guild.GetTextChannelAsync(gate.Key);
                        if (chan != null)
                        {
                            Program.P.Manager.GetRandomGates(Context.Guild.Id, ((ITextChannel)Context.Channel).IsNsfw, out int readAvailable, out int sendAvailable, out int chanReadAvailable, out int chanSendAvailable);
                            str.AppendLine(Sentences.GateOpenedIn(Context.Guild.Id, chan.Mention, chan.IsNsfw, readAvailable, sendAvailable, chanReadAvailable, chanSendAvailable));
                        }
                    }
                    if (str.Length == 0)
                        str.AppendLine(Sentences.NoGateHere(Context.Guild.Id));
                    str.AppendLine("\n" + Sentences.NbGates(Context.Guild.Id, Program.P.Manager.GetNbGates(), Program.P.Manager.GetNbGuilds()));
                    await ReplyAsync(str.ToString());
                }
            }
        }

        [Command("Ban")]
        public async Task BanGuild(string user, params string[] args)
        {
            if (Context.User.Id != Program.P.OwnerId)
            {
                await ReplyAsync(Sentences.OnlyUser(Context.Guild?.Id));
                return;
            }
            ulong userId;
            if (!ulong.TryParse(user, out userId))
            {
                await ReplyAsync(Sentences.NotUlong(Context.Guild?.Id));
                return;
            }
            string reason = string.Join(" ", args);
            if (Program.P.Manager.IsBanned(userId))
            {
                await ReplyAsync(Sentences.AlreadyBanned(Context.Guild?.Id));
                return;
            }
            await Program.P.Manager.AddBan(userId, reason);
            await ReplyAsync(Sentences.Banned(Context.Guild?.Id));
            int gatesClosed = 0;
            SocketGuildUser guildUser = null;
            foreach (var guild in Program.P.client.Guilds)
            {
                if (guildUser != null)
                {
                    guildUser = guild.Users.ToList().Find(x => x.Id == userId); // We don't download the whole list cause we will probably get ratelimited pretty quickly
                }
                if (CanModify(guild.GetUser(userId), guild.OwnerId)) // The user we want to ban is the owner of the guild
                {
                    Core.Guild guildObj = Program.P.Manager.GetGuild(Context.Guild.Id);
                    if (guildObj.DoesGateExist(Context.Channel.Id))
                    {
                        guildObj.RemoveGate(Context.Channel.Id);
                        gatesClosed++;
                    }
                }
            }
            if (guildUser != null)
            {
                await guildUser.SendMessageAsync(Sentences.UserBanned(null, reason, gatesClosed));
                if (gatesClosed > 0)
                    await ReplyAsync(Sentences.GateClosedBan(Context.Guild?.Id, gatesClosed));
            }
        }
    }
}
