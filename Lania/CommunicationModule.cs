using Discord.Commands;
using System.Threading.Tasks;

namespace Lania
{
    public class CommunicationModule : ModuleBase
    {
        [Command("Help"), Summary("Give the help"), Alias("Commands")]
        public async Task help()
        {
            await ReplyAsync(Sentences.help);
        }

        [Command("Hi"), Summary("Answer with hi"), Alias("Hey", "Hello", "Hi!", "Hey!", "Hello!")]
        public async Task SayHi()
        {
            await ReplyAsync(Sentences.hiStr);
        }
    }
}