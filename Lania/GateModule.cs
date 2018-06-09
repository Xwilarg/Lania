using Discord.Commands;
using System.IO;
using System.Threading.Tasks;

namespace Lania
{
    public class GateModule : ModuleBase
    {
        [Command("Open gate"), Summary("Open the image gate")]
        public async Task OpenGate()
        {
            File.WriteAllText("Saves/" + Context.Guild.Id + ".dat", Context.Channel.Id.ToString());
            await ReplyAsync(Sentences.gateOpened);
        }

        [Command("Close gate"), Summary("Close the image gate")]
        public async Task CloseGate()
        {
            if (Close(Context.Guild.Id))
                await ReplyAsync(Sentences.gateClosed);
            else
                await ReplyAsync(Sentences.noGate);
        }

        public static bool Close(ulong guildId)
        {
            if (File.Exists("Saves/" + guildId + ".dat"))
            {
                File.Delete("Saves/" + guildId + ".dat");
                return (true);
            }
            return (false);
        }
    }
}