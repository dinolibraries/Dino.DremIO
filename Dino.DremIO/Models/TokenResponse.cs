using Newtonsoft.Json;

namespace Dino.DremIO.Models
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Permissions
    {
        [JsonProperty("canUploadProfiles")]
        public bool CanUploadProfiles { get; set; }

        [JsonProperty("canDownloadProfiles")]
        public bool CanDownloadProfiles { get; set; }

        [JsonProperty("canEmailForSupport")]
        public bool CanEmailForSupport { get; set; }

        [JsonProperty("canChatForSupport")]
        public bool CanChatForSupport { get; set; }

        [JsonProperty("canViewAllJobs")]
        public bool CanViewAllJobs { get; set; }

        [JsonProperty("canCreateUser")]
        public bool CanCreateUser { get; set; }

        [JsonProperty("canCreateRole")]
        public bool CanCreateRole { get; set; }

        [JsonProperty("canCreateSource")]
        public bool CanCreateSource { get; set; }

        [JsonProperty("canUploadFile")]
        public bool CanUploadFile { get; set; }

        [JsonProperty("canManageNodeActivity")]
        public bool CanManageNodeActivity { get; set; }

        [JsonProperty("canManageEngines")]
        public bool CanManageEngines { get; set; }

        [JsonProperty("canManageQueues")]
        public bool CanManageQueues { get; set; }

        [JsonProperty("canManageEngineRouting")]
        public bool CanManageEngineRouting { get; set; }

        [JsonProperty("canManageSupportSettings")]
        public bool CanManageSupportSettings { get; set; }

        [JsonProperty("canConfigureSecurity")]
        public bool CanConfigureSecurity { get; set; }

        [JsonProperty("canRunDiagnostic")]
        public bool CanRunDiagnostic { get; set; }
    }

    public class TokenResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("expires")]
        public long Expires { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("admin")]
        public bool Admin { get; set; }

        [JsonProperty("clusterId")]
        public string ClusterId { get; set; }

        [JsonProperty("clusterCreatedAt")]
        public long ClusterCreatedAt { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("permissions")]
        public Permissions Permissions { get; set; }

        [JsonProperty("userCreatedAt")]
        public long UserCreatedAt { get; set; }
    }


}
