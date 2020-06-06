using Microsoft.VisualStudio.TestTools.UnitTesting;
using RssPublisher;
using System.Configuration;
using System.IO;

namespace RssPublisherTest
{
    [TestClass]
    public class DbBrokerTests
    {
        [TestMethod]
        public void InitDbTest() {
            string ConnectionString = ConfigurationManager.AppSettings["connectionString"];

            string dbFile = ConnectionString.Replace("Data Source=", "").Replace(";", "");

            File.Delete(dbFile);
            Assert.IsFalse(File.Exists(dbFile));
            
            new DbBroker(ConnectionString);

            Assert.IsTrue(File.Exists(dbFile));
            File.Delete(dbFile);
            Assert.IsFalse(File.Exists(dbFile));
        }
    }
}
