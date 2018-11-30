using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lania
{
    public class Db
    {
        public Db()
        {
            R = RethinkDB.R;
        }

        public async Task InitAsync(string dbName = "Lania")
        {
            this.dbName = dbName;
            conn = await R.Connection().ConnectAsync();
            if (!await R.DbList().Contains(dbName).RunAsync<bool>(conn))
                await R.DbCreate(dbName).RunAsync(conn);
            if (!await R.Db(dbName).TableList().Contains("Guilds").RunAsync<bool>(conn))
                await R.Db(dbName).TableCreate("Guilds").RunAsync(conn);
            if (!await R.Db(dbName).TableList().Contains("Emotes").RunAsync<bool>(conn))
                await R.Db(dbName).TableCreate("Emotes").RunAsync(conn);
            if (!await R.Db(dbName).TableList().Contains("Images").RunAsync<bool>(conn))
                await R.Db(dbName).TableCreate("Images").RunAsync(conn);
        }

        public async Task OpenGate(ulong guildId, ulong chanId)
        {
            if (await R.Db(dbName).Table("Guilds").GetAll(guildId.ToString()).Count().Eq(0).RunAsync<bool>(conn))
            {
                await R.Db(dbName).Table("Guilds").Insert(R.HashMap("id", guildId.ToString())
                   .With("chanId", chanId.ToString())
                   .With("last", "null")
                   ).RunAsync(conn);
                await R.Db(dbName).Table("Emotes").Insert(R.HashMap("id", guildId.ToString())).RunAsync(conn);
                await R.Db(dbName).Table("Images").Insert(R.HashMap("id", guildId.ToString())).RunAsync(conn);
            }
            else
                await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                   .With("chanId", chanId.ToString())
                   ).RunAsync(conn);
        }

        public async Task<bool> CloseGate(ulong guildId)
        {
            if (await R.Db(dbName).Table("Guilds").GetAll(guildId.ToString()).Count().Eq(0).RunAsync<bool>(conn))
                return (false);
            await R.Db(dbName).Table("Emotes").Filter(R.HashMap("id", guildId.ToString())).Delete().RunAsync(conn);
            await R.Db(dbName).Table("Images").Filter(R.HashMap("id", guildId.ToString())).Delete().RunAsync(conn);
            await R.Db(dbName).Table("Guilds").Filter(R.HashMap("id", guildId.ToString())).Delete().RunAsync(conn);
            return (true);
        }

        public async Task<string> GetContent(ulong guildId)
        {
            return ("**Guilds:**" + Environment.NewLine
                + JsonConvert.SerializeObject(await R.Db(dbName).Table("Guilds").Get(guildId.ToString()).RunAsync(conn))
                + Environment.NewLine + Environment.NewLine
                + "**Emotes:**" + Environment.NewLine
                + JsonConvert.SerializeObject(await R.Db(dbName).Table("Emotes").Get(guildId.ToString()).RunAsync(conn))
                + Environment.NewLine + Environment.NewLine
                + "**Images:**" + Environment.NewLine
                + JsonConvert.SerializeObject(await R.Db(dbName).Table("Images").Get(guildId.ToString()).RunAsync(conn)));
        }

        public async Task SendImage(Program.ImageData data, int counter, string url, ulong authorId)
        {
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", data.destGuild.ToString())
                .With("last", authorId + "|" + url + "|" + data.destChannel + "|" + data.destMessage + "|" + data.isChanNsfw + "|" + data.hostGuild)
                   ).RunAsync(conn);
            await R.Db(dbName).Table("Images").Update(R.HashMap("id", data.destGuild.ToString())
                .With(data.destMessage.ToString(), data.hostGuild + "|" + data.hostChannel + "|" + data.hostMessage + "|" + counter)
                   ).RunAsync(conn);
        }

        public async Task<bool> CompareChannel(ulong guildId, ulong chanId)
        {
            return ((await R.Db(dbName).Table("Guilds").Get(guildId.ToString()).RunAsync(conn))?.chanId == chanId.ToString());
        }

        public List<string> GetAllGuilds(ulong guildId, bool isNsfw, out int total, out int readAvailable)
        {
            readAvailable = 0;
            total = 0;
            List<string> ids = new List<string>();
            foreach (var elem in R.Db(dbName).Table("Guilds").Run(conn))
            {
                if (elem.id != guildId.ToString())
                {
                    SocketGuild guild = Program.p.client.Guilds.ToList().Find(x => x.Id == ulong.Parse(elem.id.ToString()));
                    ITextChannel chan = (guild != null) ? (guild.GetTextChannel(ulong.Parse(elem.chanId.ToString()))) : (null);
                    if (guild != null && chan != null && ((isNsfw && chan.IsNsfw) || !isNsfw))
                        ids.Add(elem.id.ToString());
                    else if (guild == null)
                        CloseGate(ulong.Parse(elem.id));
                    else if (guild != null && chan != null && isNsfw)
                        readAvailable++;
                }
                total++;
            }
            readAvailable += ids.Count;
            return (ids);
        }

        private RethinkDB R;
        private Connection conn;
        private string dbName;
    }
}
