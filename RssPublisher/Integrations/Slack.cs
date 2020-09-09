using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;


namespace RssPublisher.Integrations
{
    public class Slack : IPublisher
    {
        readonly HttpClient client;

        public Slack() {
            client = new HttpClient();
        }

        public void OnCompleted()
        {
            // Do nothing..
        }

        public void OnError(Exception error)
        {
            // We don't care about external errors...
        }

        public void OnNext(Story value)
        {
            PublishMessage(value.Title, value.Url).Wait();
        }

        public async Task<HttpResponseMessage> PublishMessage(string message, string url)
        {
            string slackToken = Program.GetKeyVaultAuth("slack-token");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("token", slackToken),
                new KeyValuePair<string, string>("channel", ConfigurationManager.AppSettings["slackChannel"]),
                new KeyValuePair<string, string>("unfurl_links", "false"),
                new KeyValuePair<string, string>("unfurl_media", "false"),
                new KeyValuePair<string, string>("text", message + Environment.NewLine + url)
                
            });
            var result = await client.PostAsync("https://slack.com/api/chat.postMessage", content);
            
            return result;
        }
    }
}
