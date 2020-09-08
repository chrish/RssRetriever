using RssPublisher.Integrations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace RssPublisher
{
    public class Source : IObservable<Story>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int Active { get; set; }
        public int Priority { get; set; }

        public int Ttl { get; set; }
        public int LastFetched { get; set; }

        protected DbBroker Db;

        protected List<IObserver<Story>> subscribers;

        public Source() {
            subscribers = new List<IObserver<Story>>();
            InitializeIntegrations();

            Ttl = Convert.ToInt32(ConfigurationManager.AppSettings["DefaultTtlInMinutes"]) * 60;
            LastFetched = 0;
        }

        public Source(string name, string url, int active, int priority) : this() {
            Name = name;
            Url = url;
            Active = active;
            Priority = priority;
            Db = new DbBroker();
        }

        public Source(string name, string url, int active, int priority, DbBroker db) : this()
        {
            Name = name;
            Url = url;
            Active = active;
            Priority = priority;
            Db = db;
        }

        protected void InitializeIntegrations()
        {
            var integrationsFromConfig = ConfigurationManager.AppSettings["integrations"].Split(";");

            foreach (var s in integrationsFromConfig)
            {
                subscribers.Add(new Slack());
            }
        }

        public int AddStories(List<Story> stories)
        {
            int ret = 0;

            foreach (Story s in stories)
            {
                ret += SaveStory(s);
            }

            return ret;
        }

        public long SaveNew()
        {
            Console.WriteLine("Retrieving " + this.Name);
            using (SQLiteConnection conn = Db.GetConnection())
            {
                using (var command = conn.CreateCommand())
                {
                    conn.Open();

                    string sql = "INSERT INTO SOURCE (name, url, active, priority) VALUES (@name, @url, @active, @prio)";
                    command.CommandText = sql;
                    command.Parameters.Add(new SQLiteParameter("name", Name));
                    command.Parameters.Add(new SQLiteParameter("url", Url));
                    command.Parameters.Add(new SQLiteParameter("active", Active));
                    command.Parameters.Add(new SQLiteParameter("prio", Priority));

                    command.ExecuteNonQuery();

                    command.CommandText = @"select last_insert_rowid()";
                    long lastId = (long)command.ExecuteScalar();

                    Id = Convert.ToInt32(lastId);
                    return lastId;
                }
            }
        }

        public List<Story> RetrieveAndPublish() {
            var l = Retrieve();
            foreach (var f in subscribers) {
                foreach (Story s in l) {
                    f.OnNext(s);
                    Thread.Sleep(1500);
                }
            }

            return l;
        }

        protected int UpdateTTlAndLastFetched() {
            using (SQLiteConnection conn = Db.GetConnection())
            {
                using (var command = conn.CreateCommand())
                {
                    try
                    {
                        conn.Open();

                        string sql = "UPDATE Source SET ttl=@ttl, lastFetched=@lf WHERE id=@sourceId";
                        command.CommandText = sql;
                        command.Parameters.Add(new SQLiteParameter("ttl", Ttl));
                        command.Parameters.Add(new SQLiteParameter("lf", LastFetched));
                        command.Parameters.Add(new SQLiteParameter("sourceId", Id));

                        int num = command.ExecuteNonQuery();

                        return num;
                    }
                    catch (Exception e) {
                        this.Log("System - " + e.Message);

                        return 0;
                    }
                }
            }
        }

        public bool IsRss(XDocument feed) {
            var elements = feed.Root.Elements();

            if (feed.Root.Elements("channel").Any())
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        public bool IsAtom(XDocument feed)
        {
            XNamespace ns = feed.Root.Attribute("xmlns").Value;
            
            if (feed.Root.Elements(ns + "entry").Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected List<Story> GetStoriesFromRss(XDocument feed) {
            var items = from XElement i in feed.Root.Elements("channel").Elements() where i.Name == "item" select i;
            int skipped = 0;
            List<Story> stories = new List<Story>();

            foreach (var item in items)
            {
                Story story = new Story();
                story.Title = item.Element("title").Value;


                story.Description = (string)item.Element("description") ?? "";
                story.Url = item.Element("link").Value;
                int saved = SaveStory(story);

                if (saved > 0)
                {
                    Console.WriteLine("Getting \"" + story.Title + "\"");
                    stories.Add(story);
                }
                else
                {
                    skipped++;
                }
            }

            Console.WriteLine("Skipped " + skipped + " (rss) stories from " + this.Name);
            return stories;
        }

        protected List<Story> GetStoriesFromAtom(XDocument feed)
        {
            XNamespace ns = feed.Root.Attribute("xmlns").Value;
            var items = from XElement i in feed.Root.Elements(ns + "entry") select i;
            int skipped = 0;
            List<Story> stories = new List<Story>();

            foreach (var item in items)
            {
                Story story = new Story();

                story.Title = item.Element(ns+"title").Value;
                story.Description = (string)item.Element(ns + "content") ?? "";
                story.Url = item.Element(ns + "link").Attribute("href").Value;
                
                int saved = SaveStory(story);

                if (saved > 0)
                {
                    Console.WriteLine("Getting \"" + story.Title + "\"");
                    stories.Add(story);
                }
                else
                {
                    skipped++;
                }
            }

            Console.WriteLine("Skipped " + skipped + " (atom) stories.");
            return stories;
        }

        public List<Story> Retrieve()
        {
            try
            {
                XDocument feed = XDocument.Load(Url);
                List<Story> stories = new List<Story>();
                 


                if (IsRss(feed) && feed.Root.Elements("channel").Elements("ttl").Any()) {
                    Ttl = Convert.ToInt32(feed.Root.Elements("channel").Elements("ttl").FirstOrDefault().Value) * 60;
                }

                int now = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                if (LastFetched + Ttl < now)
                {
                    if (IsRss(feed))
                    {
                        stories = GetStoriesFromRss(feed);
                    }
                    else if (IsAtom(feed))
                    {
                        stories = GetStoriesFromAtom(feed);
                    }
                    else {
                        this.Log("Cannot determine format for " + this.Url);
                    }

                    LastFetched = Convert.ToInt32((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                    UpdateTTlAndLastFetched();
                }
                else {
                    int remTtl = LastFetched + Ttl - now;
                    Console.WriteLine("Ignored \"" + this.Url + "\" (ttl remaining " + remTtl + ")");
                }

                return stories;
            }
            catch (Exception e) {
                this.Log(e.Message);
            }

            return new List<Story>();
        }

        public int SaveStory(Story story)
        {
            using (SQLiteConnection conn = Db.GetConnection())
            {
                using (var command = conn.CreateCommand())
                {
                    conn.Open();

                    string sql = "INSERT OR IGNORE INTO Story (title, description, url, source_id) VALUES (@title, @descr, @url, @sourceId)";
                    command.CommandText = sql;
                    command.Parameters.Add(new SQLiteParameter("title", story.Title));
                    command.Parameters.Add(new SQLiteParameter("descr", story.Description ?? ""));
                    command.Parameters.Add(new SQLiteParameter("url", story.Url));
                    command.Parameters.Add(new SQLiteParameter("sourceId", Id));

                    int num =  command.ExecuteNonQuery();

                    return num;
                }
            }
        }

        public List<Story> GetStories(int startingFromId)
        {
            // Hent alle stories med høyere ID
            using (SQLiteConnection conn = Db.GetConnection())
            {
                using (var command = conn.CreateCommand())
                {
                    conn.Open();

                    string sql = "SELECT title, description, url FROM STORY WHERE ID > {0}";
                    command.CommandText = sql;
                    command.Parameters.Add(startingFromId);

                    SQLiteDataReader reader = command.ExecuteReader();

                    List<Story> stories = new List<Story>();

                    while (reader.Read())
                    {
                        Story story = new Story();
                        story.Title = reader[0].ToString();
                        story.Description = reader[1].ToString();
                        story.Url = reader[2].ToString();
                        stories.Add(story);
                    }

                    return stories;
                }
            }
        }

        public int Log(string message) {
            using (SQLiteConnection conn = Db.GetConnection())
            {
                using (var command = conn.CreateCommand())
                {
                    conn.Open();

                    string sql = "INSERT INTO SourceLog (source_id, message) VALUES (@source_id, @message)";
                    command.CommandText = sql;
                    command.Parameters.Add(new SQLiteParameter("source_id", this.Id));
                    command.Parameters.Add(new SQLiteParameter("message", message));

                    int num = command.ExecuteNonQuery();

                    if (num == 0) { 
                        //TODO: Logging her
                    }

                    return num;
                }
            }
        }

        public IDisposable Subscribe(IObserver<Story> observer)
        {
            if (!subscribers.Contains(observer))
            {
                subscribers.Add(observer);
            }

            return new Unsubscriber<Story>(subscribers, observer);
        }
    }
}
