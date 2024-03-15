using Nest;

namespace Messenger.Services.ElasticServices
{
    public static class CreateElasticSearch
    {
        public static ElasticClient Create()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DisableAutomaticProxyDetection()
                .DisableDirectStreaming();

           return new ElasticClient(settings);
        }

    }
}
