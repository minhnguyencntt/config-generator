using CsvHelper.Configuration.Attributes;

namespace ConfigGenerator.Models
{
    internal class KeyOverrideMap
    {
        [Name("key")]
        public required string Key { get; set; }
        [Name("value")]
        public required string Value { get; set; }
    }
}
