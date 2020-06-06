using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RssPublisher.Integrations
{
    interface IPublisher : IObserver<Story>
    {
        public new void OnCompleted();

        public new void OnError(Exception error);

        public new void OnNext(Story value);

        public Task<HttpResponseMessage> PublishMessage(string message, string url);
    }
}
