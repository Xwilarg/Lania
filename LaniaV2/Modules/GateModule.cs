using Discord;
using Discord.Commands;
using LaniaV2.Translations;
using System.Threading.Tasks;

namespace LaniaV2.Modules
{
    [Group("Gate")]
    public class GateModule : ModuleBase
    {
        private static bool CanModify(IUser user, ulong ownerId)
        {
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
                if (guild.DoesGateExist(Context.Guild.Id))
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
        public async Task 
    }
}
