using Newtonsoft.Json;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
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
        }

        public async Task OpenGate(ulong guildId, ulong chanId)
        {
            if (await R.Db(dbName).Table("Guilds").GetAll(guildId.ToString()).Count().Eq(0).RunAsync<bool>(conn))
                await R.Db(dbName).Table("Guilds").Insert(R.HashMap("id", guildId.ToString())
                   .With("chanId", chanId.ToString())
                   ).RunAsync(conn);
            else
                await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                   .With("chanId", chanId.ToString())
                   ).RunAsync(conn);
        }

        public async Task<bool> CloseGate(ulong guildId)
        {
            if (await R.Db(dbName).Table("Guilds").GetAll(guildId.ToString()).Count().Eq(0).RunAsync<bool>(conn))
                return (false);
                await R.Db(dbName).Table("Guilds").Filter(R.HashMap("id", guildId.ToString())).Delete().RunAsync(conn);
            return (true);
        }

        public async Task<string> GetContent(ulong guildId)
        {
            return (JsonConvert.SerializeObject(await R.Db(dbName).Table("Guilds").Get(guildId.ToString()).RunAsync(conn)));
        }

        private RethinkDB R;
        private Connection conn;
        private string dbName;
    }
}
