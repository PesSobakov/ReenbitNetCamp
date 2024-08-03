using Azure.AI.TextAnalytics;

namespace ReenbitNetCamp.Services
{
    public interface IAiService
    {
        TextSentiment GetSentiment( string message);
    }
}
