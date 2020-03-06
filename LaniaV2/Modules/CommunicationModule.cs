using Discord.Commands;
using DiscordUtils;
using System.Threading.Tasks;

namespace LaniaV2.Modules
{
    public class CommunicationModule : ModuleBase
    {
        [Command("Info")]
        public async Task Info()
        {
            await ReplyAsync(embed: Utils.GetBotInfo(Program.P.StartTime, "Lania", Program.P.client.CurrentUser));
        }
    }
}
