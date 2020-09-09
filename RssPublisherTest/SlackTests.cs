using Microsoft.VisualStudio.TestTools.UnitTesting;
using RssPublisher.Integrations;

namespace RssPublisherTest
{
    [TestClass]
    public class SlackTests
    {
        /// <summary>
        /// Will retrieve feed from CNBC and check that we get more than 0 items, 
        /// and also that the first has a title, description and a link.
        /// </summary>
        [TestMethod]
        public void TestSlack()
        {
            
            Slack s = new Slack();

            var t = s.PublishMessage("This is a test message", "https://example.org");

            Assert.IsTrue(t.Result.StatusCode == System.Net.HttpStatusCode.OK);
        }
    }
}
