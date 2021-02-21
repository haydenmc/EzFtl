using System.Text.Json.Serialization;

namespace EzFtl.Models.Api
{
    public class HmacKeyResponseModel
    {
        [JsonPropertyName("hmacKey")]
        public string HmacKey { get; set; }
    }
}