using Microsoft.VisualStudio.TestTools.UnitTesting;
using RssPublisher;
using System;
using System.IO;
using System.Linq;

namespace RssPublisherTest
{
    [TestClass]
    public class RetrieveTest
    {
        /// <summary>
        /// Will retrieve feed from CNBC and check that we get more than 0 items, 
        /// and also that the first has a title, description and a link.
        /// </summary>
        [TestMethod]
        public void TestRetrieval()
        {
            string connectionString = @"Data Source=X:\code\Private\RssPublisher\RssPublisherTest\rss_RetrieveTest.db;";
            string dbFile = connectionString.Replace("Data Source=", "").Replace(";", "");

            DbBroker db = new DbBroker(connectionString);


            Source r = new Source("CNBC", "https://www.cnbc.com/id/100003114/device/rss/rss.html", 1, 1, db);
            var f = r.Retrieve();

            Assert.IsTrue(f.Count > 0);
            Assert.IsTrue(f[0].Title.Length > 0);
            Assert.IsTrue(f[0].Url.Length > 0);
            Assert.IsTrue(f[0].Description.Length > 0);

            File.Delete(dbFile);
        }

        [TestMethod]
        public void TestRetrievalAndPublish()
        {
            var f = @"X:\code\Private\RssPublisher\RssPublisherTest\rss_RetrieveTest2.db";
            if (File.Exists(f))
                File.Delete(f);
            
            string connectionString = @"Data Source=X:\code\Private\RssPublisher\RssPublisherTest\rss_RetrieveTest2.db;";
            string dbFile = connectionString.Replace("Data Source=", "").Replace(";", "");

            DbBroker db = new DbBroker(connectionString);

            Source r = new Source("CNBC", "https://www.cnbc.com/id/100003114/device/rss/rss.html", 1, 1, db);
            var stories = r.RetrieveAndPublish();

            Assert.IsTrue(stories.Count > 0);
            Assert.IsTrue(stories[0].Title.Length > 0);
            Assert.IsTrue(stories[0].Url.Length > 0);
            Assert.IsTrue(stories[0].Description.Length > 0);
        }

        [TestMethod]
        public void TestTtlUpdate()
        {
            var ff = @"X:\code\Private\RssPublisher\RssPublisherTest\rss_RetrieveTest3.db";
            if (File.Exists(ff))
                File.Delete(ff);

            string connectionString = @"Data Source=X:\code\Private\RssPublisher\RssPublisherTest\rss_RetrieveTest3.db;";
            string dbFile = connectionString.Replace("Data Source=", "").Replace(";", "");

            DbBroker db = new DbBroker(connectionString);

            Source r = new Source("CNBC", "https://www.cnbc.com/id/100003114/device/rss/rss.html", 1, 1, db);
            r.Id = 10;
            // Get default ttl

            var origTtl = r.Ttl;
            var origLf = r.LastFetched;

            var f = r.Retrieve();

            //Check ttl from feed

            var newTtl = r.Ttl;
            var newLf = r.LastFetched;

            //Get from DB
            Source dbTtl = db.GetActiveSources().Where(x => x.Id == 10).FirstOrDefault();

            if (dbTtl != null)
            {
                Assert.AreEqual(newTtl, dbTtl.Ttl);
                Assert.AreEqual(newLf, dbTtl.LastFetched);
            }
            else {
                Assert.Fail("Source not found in test db.");
            }

            Assert.IsTrue(f.Count > 0);
            Assert.AreNotEqual(origTtl, newTtl);
            Assert.AreNotEqual(origLf, newLf);

        }
    }
}
