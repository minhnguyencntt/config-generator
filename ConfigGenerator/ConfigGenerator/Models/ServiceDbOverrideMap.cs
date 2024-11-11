using CsvHelper.Configuration.Attributes;

namespace ConfigGenerator.Models
{
    internal class ServiceDbOverrideMap
    {
        [Name("svc")]
        public required string Service { get; set; }
        [Name("key")]
        public required string Key { get; set; }
        [Name("value")]
        public required string Value { get; set; }
    }
}
