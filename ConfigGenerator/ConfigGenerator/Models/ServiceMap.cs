using CsvHelper.Configuration.Attributes;

namespace ConfigGenerator.Models
{
    internal class ServiceMap
    {
        [Name("svc")]
        public required string Service { get; set; }
        [Name("folder")]
        public required string Folder { get; set; }
        [Name("config")]
        public required string Config { get; set; }
    }
}
