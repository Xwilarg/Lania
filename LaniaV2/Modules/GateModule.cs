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

        [Command("Open", RunMode = RunMode.Async)]
        public async Task OpenGate(params string[] _)
        {
            if (!CanModify(Context.User, Context.Guild.OwnerId))
                await ReplyAsync(Sentences.OnlyManage(Context.Guild.Id));
        }
    }
}
