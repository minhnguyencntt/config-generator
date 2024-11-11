namespace ConfigGenerator.Models
{
    internal class ProcessedKeyResult
    {
        public enum ProcessType
        {
            None = 0,
            Common = 1,
            Specific = 2,
            NotUse = 3
        }

        public enum ProcessKind
        {
            None = 0,
            Reference = 1,
            Override = 2,
            DbOverride = 3,
            Keep = 4
        }

        public required ProcessType Type { get; set; }
        public required ProcessKind Kind { get; set; }
        public required string Key { get; set; }
        public required string NewKey { get; set; }
        public required string OldValue { get; set; }
        public required string NewValue { get; set; }

        public override string ToString()
        {
            var missingKey = string.IsNullOrEmpty(NewKey) ? "[MISSING KEY]" : string.Empty;
            var missingValue = string.IsNullOrEmpty(NewValue) ? "[MISSING VALUE]" : string.Empty;
            return $"{missingKey} {missingValue} [{Type}] [{Kind}] [{Key}]: {OldValue} -> [{NewKey}]: {NewValue}";
        }
    }
}
