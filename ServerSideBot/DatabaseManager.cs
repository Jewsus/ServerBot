using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
                    ? (IQueryBuilder) new SqliteQueryCreator()
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

            table = new SqlTable("Channels",
                new SqlColumn("ID", MySqlDbType.Int32) {Unique = true, AutoIncrement = true, Primary = true},
                new SqlColumn("ChannelName", MySqlDbType.VarChar) {Unique = true},
                new SqlColumn("Owner", MySqlDbType.VarChar),
                new SqlColumn("Modes", MySqlDbType.Text),
                new SqlColumn("Access", MySqlDbType.Text),
                new SqlColumn("Bans", MySqlDbType.VarChar),
                new SqlColumn("Topic", MySqlDbType.Text),
                new SqlColumn("UserLimit", MySqlDbType.Int32),
                new SqlColumn("Password", MySqlDbType.Text));

            creator.EnsureExists(table);
        }

        public bool InsertPlayer(BPlayer player)
        {
            return _db.Query("INSERT INTO IgnoreLists (Name, IgnoredUsers)"
                + " VALUES (@0, @1)", player.name, string.Join(",", player.ignoredPlayers)) != 0;
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

        public void InsertChannel(Channel chan)
        {
            var accessList = ConvertDictToString(chan.accessLevels);
            _db.Query("INSERT INTO Channels (ChannelName, Owner, Modes, Access, Bans, Topic, UserLimit, Password)" +
                      " VALUES (@0, @1, @2, @3, @4, @5, @6, @7)",
                chan.name, chan.owner, chan.modes, accessList, "", chan.topic, chan.capacity, chan.password);
        }

        public void UpdateChannel(Channel chan)
        {
            var accessList = ConvertDictToString(chan.accessLevels);
            var bans = string.Join(",", chan.banList);

            _db.Query("UPDATE Channels SET Modes = @0, Access = @1, Bans = @2, Topic = @3, UserLimit = @4, " +
                      "Password = @5 WHERE ChannelName = @6",
                chan.modes, accessList, bans, chan.topic, chan.capacity, chan.password, chan.name);
        }

        public void SyncChannels()
        {
            using (var reader = _db.QueryReader("SELECT * FROM Channels"))
            {
                while (reader.Read())
                {
                    var name = reader.Get<string>("ChannelName");
                    var modes = reader.Get<string>("Modes");
                    var accessList = reader.Get<string>("Access");
                    var bans = reader.Get<string>("Bans");
                    var owner = reader.Get<string>("Owner");
                    var topic = reader.Get<string>("Topic");
                    var capacity = reader.Get<int>("UserLimit");
                    var password = reader.Get<string>("Password");

                    var access = ConvertStringToDict(accessList);
                    var chan = new Channel
                    {
                        banList = bans.Split(',').ToList(),
                        modes = modes,
                        accessLevels = access,
                        name = name,
                        owner = owner,
                        topic = topic,
                        capacity = capacity,
                        password = password
                    };

                    SSBot.channelManager.Channels.Add(chan);
                }
            }
        }

        private static string ConvertDictToString(Dictionary<string, int> dict)
        {
            var sb = new StringBuilder();
            foreach (var pair in dict)
                sb.Append(pair.Key + ":" + pair.Value);

            return sb.ToString();
        }

        private static Dictionary<string, int> ConvertStringToDict(string str)
        {
            var dict = new Dictionary<string, int>();

            var arr = str.Split(',');
            foreach (var pair in arr.Select(s => s.Split(':')))
            {
                int value;
                if (int.TryParse(pair[1], out value))
                    dict.Add(pair[0], value);
            }

            return dict;
        } 
    }
}
