using System;
using System.Collections.Generic;
using System.Threading;

namespace RssPublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();

            do
            {
                p.FetchAllFeeds();
                Console.WriteLine("Sleeping for 30 secs before fetching again...");
                Thread.Sleep(30000);
            } while (true);
        }

        public List<Story> FetchAllFeeds() {
            DbBroker db = new DbBroker();

            Console.WriteLine("Fetching feeds...");
            var sources = db.GetActiveSources();
            List<Story> aggregatedStories = new List<Story>();

            foreach(Source s in sources)
            {
                List<Story> stories = s.RetrieveAndPublish();
                aggregatedStories.AddRange(stories);
            }

            return aggregatedStories;
        }

        public Program() {
        }
    }
}
