using System.Text.Json.Serialization;

namespace EzFtl.Models.Api
{
    public class StartStreamResponseModel
    {
        [JsonPropertyName("streamId")]
        public string StreamId { get; set; }
    }
}