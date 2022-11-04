using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ada.Models
{
    public class Entity
    {
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("confidenceScore")]
        public int ConfidenceScore { get; set; }

        [JsonPropertyName("extraInformation")]
        public List<ExtraInformation> ExtraInformation { get; set; }
    }

    public class ExtraInformation
    {
        [JsonPropertyName("extraInformationKind")]
        public string ExtraInformationKind { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }
    }

    public class Root
    {
        [JsonPropertyName("entities")]
        public List<Entity> Entities { get; set; }
    }
}
