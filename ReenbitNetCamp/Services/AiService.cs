using Azure;
using Azure.AI.TextAnalytics;
using System.Reflection.Metadata;

namespace ReenbitNetCamp.Services
{
    public class AiService : IAiService
    {
        string _endpoint;
        string _apiKey;
        TextAnalyticsClient _client;

        public AiService(string endpoint, string apiKey)
        {
            _endpoint = endpoint;
            _apiKey = apiKey;
            Uri endpointUri = new(_endpoint);
            AzureKeyCredential credential = new(_apiKey);
            _client = new(endpointUri, credential);
        }
        public TextSentiment GetSentiment(string message)
        {
             Response<DocumentSentiment> response = _client.AnalyzeSentiment(message);
            DocumentSentiment docSentiment = response.Value;
            return docSentiment.Sentiment;
        }
    }
}
