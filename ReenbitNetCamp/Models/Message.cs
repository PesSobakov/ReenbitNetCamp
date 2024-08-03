using Azure.AI.TextAnalytics;

namespace ReenbitNetCamp.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string User { get; set; } = "";
        public string Reciever { get; set; } = "";
        public string Text { get; set; } = "";
        public TextSentiment Sentiment { get; set; }
        public DateTimeOffset SendTime { get; set; }
    }
}
