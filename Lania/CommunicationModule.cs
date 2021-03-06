﻿using Discord;
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
            await ReplyAsync("", false, new EmbedBuilder { Color = Color.Purple, Description = Sentences.Help(Context.Guild.Id) }.Build());
        }

        [Command("Invite")]
        public async Task Invite()
        {
            await ReplyAsync("<" + Sentences.inviteLink + ">");
        }

        [Command("Language")]
        public async Task Language(string language = null)
        {
            if (Context.User.Id != Context.Guild.OwnerId)
            {
                await ReplyAsync(Sentences.OnlyUser(Context.Guild.Id, (await Context.Guild.GetOwnerAsync()).ToString()));
                return;
            }
            if (language == null)
            {
                await ReplyAsync(Sentences.LanguageHelp(Context.Guild.Id));
                return;
            }
            string key = null;
            language = language.ToLower();
            var alternate = p.GetTranslationKeyAlternate();
            if (alternate.ContainsKey(language))
                key = language;
            else
            {
                foreach (var k in alternate)
                    if (k.Value.Contains(language))
                    {
                        key = k.Key;
                        break;
                    }
            }
            if (key == null)
                await ReplyAsync(Sentences.InvalidLanguage(Context.Guild.Id));
            else
            {
                await Program.p.GetDb().SetLanguage(Context.Guild.Id, key);
                Program.p.GetLanguages()[Context.Guild.Id] = key;
                await ReplyAsync(Sentences.LanguageChanged(Context.Guild.Id));
            }
        }

        [Command("Infos"), Summary("Give informations about the bot"), Alias("Info")]
        public async Task Infos()
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Color = Color.Purple
            };
            embed.AddField(Sentences.Author(Context.Guild.Id), "Zirk#0001");
            embed.AddField(Sentences.Uptime(Context.Guild.Id), Program.TimeSpanToString(Context.Guild.Id, DateTime.Now.Subtract(p.startTime)));
            embed.AddField(Sentences.LatestVersion(Context.Guild.Id), new FileInfo(Assembly.GetEntryAssembly().Location).LastWriteTimeUtc.ToString(Sentences.DateTimeFormat(Context.Guild.Id)) + " UTC+0", true);
            embed.AddField("GitHub", "https://github.com/Xwilarg/Lania");
            embed.AddField(Sentences.InvitationLink(Context.Guild.Id), Sentences.inviteLink);
            await ReplyAsync("", false, embed.Build());
        }
    }
}