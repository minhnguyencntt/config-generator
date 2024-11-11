using CsvHelper.Configuration.Attributes;

namespace ConfigGenerator.Models
{
    internal class AclMap
    {
        [Name("svc")]
        public required string Service { get; set; }
        [Name("dbdUser")]
        public string? DbdUser { get; set; }
        [Name("logUser")]
        public string? LogUser { get; set; }
        [Name("logSchema")]
        public string? LogSchema { get; set; }
        [Name("workflowUser")]
        public string? WorkflowUser { get; set; }
        [Name("workflowSchema")]
        public string? WorkflowSchema { get; set; }
    }
}
