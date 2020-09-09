using System;
using System.Collections.Generic;
using System.Threading;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using ClientCredential = Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential;

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

        public static string GetKeyVaultAuth(string tokenName) {
            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                 }
            };

            string keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
            var envVars = Environment.GetEnvironmentVariables();

            string clientId = Environment.GetEnvironmentVariable("akvClientId");
            string clientSecret = Environment.GetEnvironmentVariable("akvClientSecret");

            KeyVaultClient kvClient = new KeyVaultClient(async (authority, resource, scope) =>
            {
                var adCredential = new ClientCredential(clientId, clientSecret);
                var authenticationContext = new AuthenticationContext(authority, null);
                return (await authenticationContext.AcquireTokenAsync(resource, adCredential)).AccessToken;
            });

            var client = new SecretClient(new Uri("https://" +keyVaultName+ ".vault.azure.net/"), new DefaultAzureCredential(), options);

            KeyVaultSecret secret = client.GetSecret(tokenName);   

            return secret.Value;
        }
    }
}
