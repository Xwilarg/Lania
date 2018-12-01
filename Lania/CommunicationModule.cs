using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Lania
{
    public class CommunicationModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Help"), Summary("Give the help"), Alias("Commands")]
        public async Task Help()
        {
            await ReplyAsync("", false, new EmbedBuilder { Color = Color.Purple, Description = Sentences.help }.Build());
        }

        [Command("Hi"), Summary("Answer with hi"), Alias("Hey", "Hello", "Hi!", "Hey!", "Hello!")]
        public async Task SayHi()
        {
            await ReplyAsync(Sentences.hiStr);
        }

        [Command("Invite")]
        public async Task Invite()
        {
            await ReplyAsync("<https://discordapp.com/oauth2/authorize?client_id=454742499085254656&permissions=83968&scope=bot>");
        }

        [Command("Infos"), Summary("Give informations about the bot"), Alias("Info")]
        public async Task Infos()
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Color = Color.Purple
            };
            embed.AddField(Sentences.author, "Zirk#0001");
            embed.AddField(Sentences.uptime, Program.TimeSpanToString(DateTime.Now.Subtract(p.startTime)));
            embed.AddField(Sentences.latestVersion, new FileInfo(Assembly.GetEntryAssembly().Location).LastWriteTimeUtc.ToString(Sentences.dateTimeFormat) + " UTC+0", true);
            embed.AddField("GitHub", "https://github.com/Xwilarg/Lania");
            embed.AddField(Sentences.invitationLink, "https://discordapp.com/oauth2/authorize?client_id=454742499085254656&permissions=83968&scope=bot");
            await ReplyAsync("", false, embed.Build());
        }
    }
}