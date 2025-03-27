using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Text;

namespace Dino.DremIO.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class JobResultReponse: JobResultReponse<Dictionary<string,object>>
    {
    }
    public class JobResultReponse<TModel>
    {
        [JsonProperty("rowCount")]
        public int RowCount { get; set; }

        [JsonProperty("schema")]
        public List<Schema> Schema { get; set; }

        [JsonProperty("rows")]
        public List<TModel> Rows { get; set; }
    }

    public class Schema
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public Type Type { get; set; }
    }

    public class Type
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }


}
