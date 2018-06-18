using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Lania
{
    [Group("Gate")]
    public class GateModule : ModuleBase
    {
        [Command("Open"), Summary("Open the image gate")]
        public async Task OpenGate()
        {
            if (Context.Guild.OwnerId != Context.User.Id)
                await ReplyAsync(Sentences.OnlyUser((await Context.Guild.GetOwnerAsync()).ToString()));
            else
            {
                if (!Directory.Exists("Saves/Guilds"))
                    Directory.CreateDirectory("Saves/Guilds");
                File.WriteAllText("Saves/Guilds/" + Context.Guild.Id + ".dat", Context.Channel.Id.ToString());
                await ReplyAsync(Sentences.gateOpened);
            }
        }

        [Command("Close"), Summary("Close the image gate")]
        public async Task CloseGate()
        {
            if (Context.Guild.OwnerId != Context.User.Id)
                await ReplyAsync(Sentences.OnlyUser((await Context.Guild.GetOwnerAsync()).ToString()));
            else if (Close(Context.Guild.Id))
                await ReplyAsync(Sentences.gateClosed);
            else
                await ReplyAsync(Sentences.noGate);
        }

        [Command("Stats"), Summary("Stats about emotes received")]
        public async Task StatsGate()
        {
            if (Directory.Exists("Saves/Emotes") && Directory.Exists("Saves/Emotes/" + Context.Guild.Id))
            {
                Dictionary<string, int> allEmotes = new Dictionary<string, int>();
                foreach (string f in Directory.GetFiles("Saves/Emotes/" + Context.Guild.Id))
                {
                    FileInfo fi = new FileInfo(f);
                    int nb = Convert.ToInt32(File.ReadAllText(f));
                    if (nb > 0)
                        allEmotes.Add(fi.Name.Split('.')[0].Replace('-', ':'), nb);
                }
                if (allEmotes.Count == 0)
                    await ReplyAsync(Sentences.noEmote);
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
                    await ReplyAsync(Sentences.myEmotes + Environment.NewLine + finalStr);
                }
            }
            else
                await ReplyAsync(Sentences.noEmote);
        }

        [Command("Status"), Summary("Get informations about the gate")]
        public async Task StatusGate()
        {
            string finalStr = "";
            if (File.Exists("Saves/Guilds/" + Context.Guild.Id + ".dat"))
                finalStr += Sentences.GateChannel("<#" + File.ReadAllText("Saves/Guilds/" + Context.Guild.Id + ".dat") + ">");
            else
                finalStr += Sentences.noGateHere;
            await ReplyAsync(finalStr + Environment.NewLine + Sentences.NbGates(Directory.GetFiles("Saves/Guilds").Length.ToString()));
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