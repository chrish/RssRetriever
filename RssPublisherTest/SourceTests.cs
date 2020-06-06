using Microsoft.VisualStudio.TestTools.UnitTesting;
using RssPublisher;
using System.IO;

namespace RssPublisherTest
{
    [TestClass]
    public class SourceTests
    {
        [TestMethod]
        public void AddSourceTest() {
            string connectionString = @"Data Source=X:\code\Private\RssPublisher\RssPublisherTest\rss_AddSourceTest.db;";
            string dbFile = connectionString.Replace("Data Source=", "").Replace(";", "");

            if (File.Exists(dbFile))
            {
                File.Delete(dbFile);
            }

            DbBroker db = new DbBroker(connectionString);

            var sources = db.GetActiveSources();
            Source s = new Source("Test", "https://example.org", 1, 99, db);
            s.SaveNew();
            
            var sources2 = db.GetActiveSources();

            Assert.IsTrue(sources.Count < sources2.Count);
            Assert.AreEqual(1, sources2.Count - sources.Count);

        }

        [TestMethod]
        public void AddStoryTest()
        {
            string connectionString = @"Data Source=X:\code\Private\RssPublisher\RssPublisherTest\rss_AddStoryTest.db;";
            string dbFile = connectionString.Replace("Data Source=", "").Replace(";", "");

            if (File.Exists(dbFile))
            {
                File.Delete(dbFile);
            }

            DbBroker db = new DbBroker(connectionString);
            
            Source s = new Source("CNN", "https://www.cnbc.com/id/100003114/device/rss/rss.html", 1, 1, db);

            var f = s.Retrieve();

            Assert.IsTrue(f.Count > 0);

        }

        [TestMethod]
        public void AddDuplicateStoryTest()
        {
            string connectionString = @"Data Source=X:\code\Private\RssPublisher\RssPublisherTest\rss_AddDupStoryTest.db;";
            string dbFile = connectionString.Replace("Data Source=", "").Replace(";", "");
            
            if (File.Exists(dbFile))
            {
                File.Delete(dbFile);
            }
            
            DbBroker db = new DbBroker(connectionString);

            Source r = new Source("CNN", "https://www.cnbc.com/id/100003114/device/rss/rss.html", 1, 1, db);
            var f = r.Retrieve();

            int result = r.SaveStory(f[0]);

            Assert.IsTrue(result == 0);

            
        }

        [TestMethod]
        public void SourceLoggingTest() {
            string connectionString = @"Data Source=X:\code\Private\RssPublisher\RssPublisherTest\rss_AddSourceLog.db;";
            string dbFile = connectionString.Replace("Data Source=", "").Replace(";", "");

            if (File.Exists(dbFile))
            {
                File.Delete(dbFile);
            }

            DbBroker db = new DbBroker(connectionString);

            Source r = new Source("CNN", "https://www.cnbc.com/id/100003114/device/rss/rss.html", 1, 1, db);
            var result = r.Log("This is a log entry from the unit tests...");
            
            Assert.IsTrue(result == 1);
        }
    }
}
