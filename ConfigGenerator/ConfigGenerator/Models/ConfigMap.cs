using CsvHelper.Configuration.Attributes;

namespace ConfigGenerator.Models
{
    internal class ConfigMap
    {
        [Name("ENV_KEY")]
        public required string Key { get; set; }
        [Name("ENV_VALUE")]
        public string? Value { get; set; }
    }
}
