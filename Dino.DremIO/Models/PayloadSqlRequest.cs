using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dino.DremIO.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class NessieSource
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class PayloadSqlRequest
    {
        [JsonProperty("sql")]
        public string Sql { get; set; }

        [JsonProperty("context")]
        public IEnumerable<string> Context { get; set; }

        [JsonProperty("references")]
        public Dictionary<string, NessieSource> References { get; set; }
    }

}
