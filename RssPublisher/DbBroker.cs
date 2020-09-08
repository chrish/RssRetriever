using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace RssPublisher
{
    public class DbBroker
    {
        protected string ConnectionString;

        protected Configuration config;
        public DbBroker() {

            var dllName = Assembly.GetExecutingAssembly().GetName().Name;
            var dllPath = Assembly.GetExecutingAssembly().Location;
            config = ConfigurationManager.OpenExeConfiguration(dllPath);
            ConnectionString = config.AppSettings.Settings["connectionString"].Value;
            

            InitializeDatabaseIfNotExisting();
        }

        public DbBroker(string connectionString)
        {
            ConnectionString = connectionString;

            InitializeDatabaseIfNotExisting();
        }

        public SQLiteConnection GetConnection() {
            return new SQLiteConnection(ConnectionString);
        }

        protected void InitializeDatabaseIfNotExisting() {
            string dbFile = ConnectionString.Replace("Data Source=", "").Replace(";", "");

            if (!File.Exists(dbFile))
            {
                SQLiteConnection.CreateFile(dbFile);
                string Ddl = File.ReadAllText(@"db\ddl.sql");
                string Dml = File.ReadAllText(@"db\dml.sql");

                using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
                {
                    using (var command = conn.CreateCommand())
                    {
                        conn.Open();
                        command.CommandText = Ddl;
                        command.ExecuteNonQuery();

                        command.CommandText = Dml;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public List<Source> GetActiveSources() {
            using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
            {
                using (var command = conn.CreateCommand())
                {
                    conn.Open();

                    string sql = "SELECT id, name, url, active, priority, ttl, lastFetched FROM Source WHERE active = 1 order by priority desc";
                    command.CommandText = sql;
                    SQLiteDataReader reader = command.ExecuteReader();

                    List<Source> sources = new List<Source>();

                    while (reader.Read())
                    {
                        Source source = new Source(reader[1].ToString(), reader[2].ToString(), Convert.ToInt32(reader[3]), Convert.ToInt32(reader[4]), this);
                        source.Id = Convert.ToInt32(reader[0]);
                        
                        if (reader[5] != DBNull.Value)
                        {
                            source.Ttl = Convert.ToInt32(reader[5]);
                        }
                        else 
                        {
                            source.Ttl = 0;
                        }

                        if (reader[6] != DBNull.Value)
                        {
                            source.LastFetched = Convert.ToInt32(reader[6]);
                        }
                        else
                        {
                            source.LastFetched = 0;
                        }
                        
                        sources.Add(source);
                    }

                    return sources;
                }
            }
        }
    }
}
