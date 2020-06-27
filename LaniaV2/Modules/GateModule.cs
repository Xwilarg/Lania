using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
        public async Task OpenGate(params string[] _)
        {
            if (!CanModify(Context.User, Context.Guild.OwnerId))
                await ReplyAsync(Sentences.OnlyManage(Context.Guild.Id));
            else if (Program.P.Manager.IsBanned(Context.User.Id))
                await ReplyAsync(Sentences.IsBanned(Context.Guild.Id));
            else
            {
                Core.Guild guild = Program.P.Manager.GetGuild(Context.Guild.Id);
                if (guild.DoesGateExist(Context.Channel.Id))
                    await ReplyAsync(Sentences.GateAlredyOpened(Context.Guild.Id));
                else if (guild.DidReachMaxLimitGate())
                    await ReplyAsync(Sentences.TooManyGatesOpened(Context.Guild.Id, guild.GetMaxLimitGate()));
                else
                {
                    guild.AddGate(Context.Channel.Id);
                    await ReplyAsync(Sentences.GateOpened(Context.Guild.Id));
                }
            }
        }

        [Command("Close")]
        public async Task CloseGate(params string[] _)
        {
            if (!CanModify(Context.User, Context.Guild.OwnerId))
                await ReplyAsync(Sentences.OnlyManage(Context.Guild.Id));
            else if (Program.P.Manager.IsBanned(Context.User.Id))
                await ReplyAsync(Sentences.IsBanned(Context.Guild.Id));
            else
            {
                Core.Guild guild = Program.P.Manager.GetGuild(Context.Guild.Id);
                if (!guild.DoesGateExist(Context.Channel.Id))
                    await ReplyAsync(Sentences.NoGate(Context.Guild.Id));
                else
                {
                    guild.RemoveGate(Context.Channel.Id);
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
                            str.AppendLine(Sentences.GateOpenedIn(Context.Guild.Id, chan.Mention, chan.IsNsfw) + "\n");
                        }
                    }
                    if (str.Length == 0)
                        await ReplyAsync(Sentences.NoGateHere(Context.Guild.Id));
                    else
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
