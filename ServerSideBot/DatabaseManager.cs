using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace ServerSideBot
{
    public class DManager
    {
        private readonly IDbConnection _db;

        public DManager(IDbConnection db)
        {
            _db = db;

            var creator = new SqlTableCreator(db,
                                             db.GetSqlType() == SqlType.Sqlite
                                             ? (IQueryBuilder)new SqliteQueryCreator()
                                             : new MysqlQueryCreator());

            var table = new SqlTable("IgnoreLists",
                new SqlColumn("ID", MySqlDbType.Int32) {AutoIncrement = true, Primary = true},
                new SqlColumn("Name", MySqlDbType.VarChar) {Unique = true},
                new SqlColumn("IgnoredUsers", MySqlDbType.Text));

            creator.EnsureExists(table);

            table = new SqlTable("AutomatedReplies",
                new SqlColumn("Received", MySqlDbType.Text) {Unique = true},
                new SqlColumn("Reply", MySqlDbType.Text));

            creator.EnsureExists(table);
        }

        public bool InsertPlayer(BPlayer player)
        {
            return _db.Query("INSERT INTO IgnoreLists (Name, IgnoredUsers)"
                + " VALUES (@0, @1)", player.name, string.Join(",", player.ignoredPlayers)) != 0;
        }

        public bool DeletePlayer(string player)
        {
            return _db.Query("DELETE FROM IgnoreLists WHERE Name = @0", player) != 0;
        }

        public bool SavePlayer(BPlayer player)
        {
            return _db.Query("UPDATE IgnoreLists SET IgnoredUsers = @0 WHERE Name = @1",
                string.Join(",", player.ignoredPlayers), player.name) != 0;
        }

        public Dictionary<string, string> GetAutomatedReplies()
        {
            var dict = new Dictionary<string, string>();
            using (var reader = _db.QueryReader("SELECT * FROM AutomatedReplies"))
            {
                while (reader.Read())
                {
                    var msg = reader.Get<string>("Received");
                    var reply = reader.Get<string>("Reply");

                    dict.Add(msg, reply);
                }
            }
            return dict;
        }

        public void SyncPlayers()
        {
            using (var reader = _db.QueryReader("SELECT * FROM IgnoreLists"))
            {
                while (reader.Read())
                {
                    var name = reader.Get<string>("Name");
                    var ignores = reader.Get<string>("IgnoredUsers");

                    SSBot.Storage.players.Add(new BPlayer(name)
                    {
                        ignoredPlayers = ignores.Split(',').ToList()
                    });
                }
            }
        }
    }
}
