using LaniaV2.Core;
using Newtonsoft.Json.Linq;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System.Threading.Tasks;

namespace LaniaV2.Db
{
    public class Db
    {
        public async Task InitAsync(string dbName = "Lania")
        {
            R = RethinkDB.R;
            this.dbName = dbName;
            conn = await R.Connection().ConnectAsync();
            if (!await R.DbList().Contains(dbName).RunAsync<bool>(conn))
                await R.DbCreate(dbName).RunAsync(conn);
            if (!await R.Db(dbName).TableList().Contains("Guilds").RunAsync<bool>(conn))
                await R.Db(dbName).TableCreate("Guilds").RunAsync(conn);
            if (!await R.Db(dbName).TableList().Contains("Bans").RunAsync<bool>(conn))
                await R.Db(dbName).TableCreate("Bans").RunAsync(conn);
            foreach (JProperty p in await R.Db(dbName).Table("Bans").RunAsync(conn))
                System.Console.WriteLine(p.Name + " ; " + p.Value);
        }

        public async Task InitGuildAsync(ulong guildId)
        {
            Guild guild;
            if (await R.Db(dbName).Table("Guilds").GetAll(guildId.ToString()).Count().Eq(0).RunAsync<bool>(conn))
            {
                guild = new Guild(guildId);
                await R.Db(dbName).Table("Guilds").Insert(guild).RunAsync(conn);
            }
            else
                guild = await R.Db(dbName).Table("Guilds").Get(guildId.ToString()).RunAsync<Guild>(conn);
            Program.P.Manager.AddGuild(guildId, guild);
        }

        public async Task UpdateGuild(Guild guild)
        {
            await R.Db(dbName).Table("Guilds").Update(guild).RunAsync(conn);
        }

        public async Task RemoveGuild(Guild guild)
        {
            await R.Db(dbName).Table("Guilds").Get(guild).Delete().RunAsync(conn);
        }

        public async Task AddBan(ulong userId, string reason)
        {
            await R.Db(dbName).Table("Bans").Insert(R.HashMap("id", userId.ToString())
                .With("Reason", reason)
                ).RunAsync(conn);
        }

        private RethinkDB R;
        private Connection conn;
        private string dbName;
    }
}
