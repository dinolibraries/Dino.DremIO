using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dino.DremIO.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Acceleration
    {
        [JsonProperty("reflectionRelationships")]
        public List<ReflectionRelationship> ReflectionRelationships { get; set; }
    }

    public class ReflectionRelationship
    {
        [JsonProperty("datasetId")]
        public string DatasetId { get; set; }

        [JsonProperty("reflectionId")]
        public string ReflectionId { get; set; }

        [JsonProperty("relationship")]
        public string Relationship { get; set; }
    }

    public enum JobState
    {
        COMPLETED, CANCELED, FAILED, RUNNING
    }
    public class JobGetResponse
    {
        [JsonProperty("jobState")]
        public JobState? JobState { get; set; }

        [JsonProperty("rowCount")]
        public int RowCount { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty("startedAt")]
        public DateTime StartedAt { get; set; }

        [JsonProperty("endedAt")]
        public DateTime EndedAt { get; set; }

        [JsonProperty("acceleration")]
        public Acceleration Acceleration { get; set; }

        [JsonProperty("queryType")]
        public string QueryType { get; set; }

        [JsonProperty("queueName")]
        public string QueueName { get; set; }

        [JsonProperty("queueId")]
        public string QueueId { get; set; }

        [JsonProperty("resourceSchedulingStartedAt")]
        public DateTime ResourceSchedulingStartedAt { get; set; }

        [JsonProperty("resourceSchedulingEndedAt")]
        public DateTime ResourceSchedulingEndedAt { get; set; }

        [JsonProperty("cancellationReason")]
        public string CancellationReason { get; set; }
    }


}
