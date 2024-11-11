using Newtonsoft.Json;

namespace ConfigGenerator.Models
{


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Egress
    {
        [JsonProperty("to")]
        public List<To> To { get; set; }

        [JsonProperty("ports")]
        public List<PortConfig> Ports { get; set; }
    }

    public class From
    {
        [JsonProperty("podSelector")]
        public PodSelector PodSelector { get; set; }
    }

    public class Ingress
    {
        [JsonProperty("from")]
        public List<From> From { get; set; }
    }

    public class IpBlock
    {
        [JsonProperty("cidr")]
        public string Cidr { get; set; }
    }

    public class MatchLabels
    {
        [JsonProperty("role-legacy")]
        public string RoleLegacy { get; set; }

        [JsonProperty("role-orchestration")]
        public string RoleOrchestration { get; set; }

        [JsonProperty("role-domain")]
        public string RoleDomain { get; set; }

        [JsonProperty("app.kubernetes.io/instance")]
        public string AppKubernetesIoInstance { get; set; }

        [JsonProperty("app.kubernetes.io/name")]
        public string AppKubernetesIoName { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("namespace")]
        public string Namespace { get; set; }
    }

    public class PodSelector
    {
        [JsonProperty("matchLabels")]
        public MatchLabels MatchLabels { get; set; }
    }

    public class PortConfig
    {
        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }
    }

    public class NetpolConfig
    {
        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("spec")]
        public Spec Spec { get; set; }
    }

    public class Spec
    {
        [JsonProperty("podSelector")]
        public PodSelector PodSelector { get; set; }

        [JsonProperty("policyTypes")]
        public List<string> PolicyTypes { get; set; }

        [JsonProperty("ingress")]
        public List<Ingress> Ingress { get; set; }

        [JsonProperty("egress")]
        public List<Egress> Egress { get; set; }
    }

    public class To
    {
        [JsonProperty("ipBlock")]
        public IpBlock IpBlock { get; set; }

        [JsonProperty("podSelector")]
        public PodSelector PodSelector { get; set; }
    }


}
