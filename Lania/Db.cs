using Discord;
using Discord.WebSocket;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
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
            if (!await R.Db(dbName).TableList().Contains("Bans").RunAsync<bool>(conn))
                await R.Db(dbName).TableCreate("Bans").RunAsync(conn);
            if (!await R.Db(dbName).TableList().Contains("Languages").RunAsync<bool>(conn))
                await R.Db(dbName).TableCreate("Languages").RunAsync(conn);
        }

        public async Task OpenGate(ulong guildId, ulong chanId)
        {
            if (!await DoesGuildExist(guildId))
            {
                await R.Db(dbName).Table("Guilds").Insert(R.HashMap("id", guildId.ToString())
                   .With("chanId", chanId.ToString())
                   ).RunAsync(conn);
                await R.Db(dbName).Table("Emotes").Insert(R.HashMap("id", guildId.ToString())).RunAsync(conn);
                await R.Db(dbName).Table("Images").Insert(R.HashMap("id", guildId.ToString())
                   .With("last", "null")
                    ).RunAsync(conn);
            }
            else
                await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                   .With("chanId", chanId.ToString())
                   ).RunAsync(conn);
        }

        public async Task<bool> CloseGate(ulong guildId)
        {
            if (!await DoesGuildExist(guildId))
                return (false);
            await R.Db(dbName).Table("Guilds").Filter(R.HashMap("id", guildId.ToString())).Delete().RunAsync(conn);
            return (true);
        }

        public async Task InitGuild(ulong guildId)
        {
            if (await R.Db(dbName).Table("Languages").GetAll(guildId.ToString()).Count().Eq(0).RunAsync<bool>(conn))
                await R.Db(dbName).Table("Languages").Insert(R.HashMap("id", guildId.ToString())
                   .With("language", "en")
                    ).RunAsync(conn);
        }

        public async Task<bool> DoesGuildExist(ulong guildId)
        {
            return (!await R.Db(dbName).Table("Guilds").GetAll(guildId.ToString()).Count().Eq(0).RunAsync<bool>(conn));
        }

        public async Task SendImage(Program.ImageData data, int counter, string url, ulong authorId)
        {
            await R.Db(dbName).Table("Images").Update(R.HashMap("id", data.destGuild.ToString())
                .With("last", authorId + "|" + url + "|" + data.destChannel + "|" + data.destMessage + "|" + data.isChanNsfw + "|" + data.hostGuild)
                .With(data.destMessage.ToString(), data.hostGuild + "|" + data.hostChannel + "|" + data.hostMessage + "|" + counter)
                   ).RunAsync(conn);
        }

        public async Task<string> GetGateChan(ulong guildId)
        {
            return ((await R.Db(dbName).Table("Guilds").Get(guildId.ToString()).RunAsync(conn))?.chanId);
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

        public async Task<string> GetLast(ulong guildId)
        {
            string last = (await R.Db(dbName).Table("Images").Get(guildId.ToString()).RunAsync(conn))?.last;
            if (last == "null")
                return (null);
            return (last);
        }

        public async Task<string> GetImage(ulong guildId, ulong messageId)
        {
            dynamic json = await R.Db(dbName).Table("Images").Get(guildId.ToString()).RunAsync(conn);
            if (json != null)
                return (json[messageId.ToString()]);
            return (null);
        }

        public async Task<IEnumerable<dynamic>> GetEmotes(ulong guildId)
        {
            return (await R.Db(dbName).Table("Emotes").Get(guildId.ToString()).RunAsync(conn));
        }

        public async Task AddReaction(ulong guildId, string reaction, int nbIncrease)
        {
            dynamic json = (await R.Db(dbName).Table("Emotes").Get(guildId.ToString()).RunAsync(conn));
            string reactionStr = null;
            if (json != null)
                reactionStr = json[reaction];
            int reactionNb = 0;
            if (reactionStr != null)
                reactionNb = int.Parse(reactionStr);
            if (reactionNb + nbIncrease == 0)
                await R.Db(dbName).Table("Emotes").Get(guildId.ToString()).Replace(x => x.Without(reaction)).RunAsync(conn);
            else
                await R.Db(dbName).Table("Emotes").Update(R.HashMap("id", guildId.ToString())
                    .With(reaction, (reactionNb + nbIncrease).ToString())
                    ).RunAsync(conn);
        }

        public async Task Ban(string userId, string reason)
        {
            await R.Db(dbName).Table("Bans").Insert(R.HashMap("id", userId.ToString())
                .With("reason", reason)
                ).RunAsync(conn);
        }

        public async Task<bool> IsBan(string userId)
        {
            return (!await R.Db(dbName).Table("Bans").GetAll(userId.ToString()).Count().Eq(0).RunAsync<bool>(conn));
        }

        public async Task DeleteLast(ulong guildId)
        {
            await R.Db(dbName).Table("Images").Update(R.HashMap("id", guildId.ToString())
                .With("last", "null")
                ).RunAsync(conn);
        }

        public async Task<string> GetLanguage(ulong guildId)
        {
            return ((await R.Db(dbName).Table("Languages").Get(guildId.ToString()).RunAsync(conn)).language);
        }

        private RethinkDB R;
        private Connection conn;
        private string dbName;
    }
}
