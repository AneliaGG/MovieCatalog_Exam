using System.Text.Json.Serialization;

namespace MovieCatalog_Exam.DTOs
{
    public class MovieDTO
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
