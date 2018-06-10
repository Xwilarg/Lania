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
            if (Context.Guild.OwnerId != Context.User.Id)
                await ReplyAsync(Sentences.onlyUser((await Context.Guild.GetOwnerAsync()).ToString()));
            else
            {
                if (!Directory.Exists("Saves/Guilds"))
                    Directory.CreateDirectory("Saves/Guilds");
                File.WriteAllText("Saves/Guilds/" + Context.Guild.Id + ".dat", Context.Channel.Id.ToString());
                await ReplyAsync(Sentences.gateOpened);
            }
        }

        [Command("Close gate"), Summary("Close the image gate")]
        public async Task CloseGate()
        {
            if (Context.Guild.OwnerId != Context.User.Id)
                await ReplyAsync(Sentences.onlyUser((await Context.Guild.GetOwnerAsync()).ToString()));
            else if (Close(Context.Guild.Id))
                await ReplyAsync(Sentences.gateClosed);
            else
                await ReplyAsync(Sentences.noGate);
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
    }
}