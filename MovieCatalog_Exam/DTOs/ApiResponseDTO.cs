using System.Text.Json.Serialization;

namespace MovieCatalog_Exam.DTOs
{
    public class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }
        [JsonPropertyName("movie")]
        public MovieDTO Movie { get; set; } = new MovieDTO();
    }
}
