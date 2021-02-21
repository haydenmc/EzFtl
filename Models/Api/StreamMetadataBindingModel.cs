using System.Text.Json.Serialization;

namespace EzFtl.Models.Api
{
    public class StreamMetadataBindingModel
    {
        [JsonPropertyName("audioCodec")]
        public string AudioCodec { get; set; }

        [JsonPropertyName("ingestServer")]
        public string IngestServer { get; set; }

        [JsonPropertyName("ingestViewers")]
        public int IngestViewers { get; set; }

        [JsonPropertyName("lostPackets")]
        public int LostPackets { get; set; }

        [JsonPropertyName("nackPackets")]
        public int NackPackets { get; set; }

        [JsonPropertyName("recvPackets")]
        public int ReceivedPackets { get; set; }

        [JsonPropertyName("sourceBitrate")]
        public int SourceBitrateBitsPerSecond { get; set; }

        [JsonPropertyName("sourcePing")]
        public int SourcePing { get; set; }

        [JsonPropertyName("streamTimeSeconds")]
        public int StreamTimeSeconds { get; set; }

        [JsonPropertyName("vendorName")]
        public string VendorName { get; set; }

        [JsonPropertyName("vendorVersion")]
        public string VendorVersion { get; set; }

        [JsonPropertyName("videoCodec")]
        public string VideoCodec { get; set; }

        [JsonPropertyName("videoHeight")]
        public int VideoHeight { get; set; }

        [JsonPropertyName("videoWidth")]
        public int VideoWidth { get; set; }
    }
}