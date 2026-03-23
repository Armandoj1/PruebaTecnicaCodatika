using System.Text.Json.Serialization;

namespace BookRadar.Models
{
    public class OpenLibraryResponse
    {
        [JsonPropertyName("docs")]
        public List<OpenLibraryDoc> Docs { get; set; } = new();
    }

    public class OpenLibraryDoc
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("first_publish_year")]
        public int? FirstPublishYear { get; set; }

        [JsonPropertyName("publisher")]
        public List<string>? Publisher { get; set; }
    }
}
