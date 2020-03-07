using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System.Collections.Generic;
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
            Languages = new Dictionary<ulong, string>();
        }

        private RethinkDB R;
        private Connection conn;
        private string dbName;

        public Dictionary<ulong, string> Languages { private set; get; }
    }
}
