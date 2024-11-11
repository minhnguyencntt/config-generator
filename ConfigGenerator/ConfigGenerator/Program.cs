using System.Globalization;
using System.Text;
using ConfigGenerator;
using ConfigGenerator.Models;
using CsvHelper;
using CsvHelper.Configuration;
using dotenv.net;
using Newtonsoft.Json;
using static ConfigGenerator.Constants;

namespace MyApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //ProcessConfigs(args);

            ProcessNetpol(args);
        }

        private static void ProcessNetpol(string[] args)
        {
            var files = Directory.GetFiles(Path.Combine(Constants.Workspace, "netpol"));
            var logs = new StringBuilder();
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                var config = JsonConvert.DeserializeObject<NetpolConfig>(content);
                if (config != null && config.Spec != null && config.Spec.Egress != null && config.Spec.Egress.Any())
                {

                    logs.AppendLine($"=========={file}==========");
                    logs.AppendLine($"# egress keys loaded: {config.Spec.Egress.Count}");

                    foreach (var item in config.Spec.Egress)
                    {
                        if (item.To != null && item.To.Any() && item.Ports != null && item.Ports.Any())
                        {
                            logs.AppendLine($"[IP]: {item.To[0].IpBlock.Cidr} [Port]: {item.Ports[0].Port}");
                        }
                    }
                    logs.AppendLine("==========");
                    logs.AppendLine();
                }
            }

            using StreamWriter writer = new(Path.Combine(Constants.Workspace, "netpol", $"log.{DateTime.Now:ddMMyyyyhhmm}"));
            writer.Write(logs.ToString());
        }

        private static void ProcessConfigs(string[] args)
        {
            var serviceMap = new Dictionary<string, ServiceMap>();
            var configMap = new Dictionary<string, string>();
            var keyOverrideMap = new Dictionary<string, string>();
            var serviceDbOverrideMap = new Dictionary<string, List<ServiceDbOverrideMap>>();

            LoadSvcMap(ref serviceMap);
            LoadConfigMap(ref configMap);
            LoadKeyOverrideMap(ref keyOverrideMap);
            LoadServiceDbOverrideMap(ref serviceDbOverrideMap);

            foreach (var service in serviceMap.Values)
            {
                var processedKeyResult = new List<ProcessedKeyResult>();
                var logs = new StringBuilder();
                var envVars = LoadEnv(service, ref logs);
                var finalEnvVars = new Dictionary<string, string>();
                foreach (var envVar in envVars)
                {
                    bool hasProcessed = false;
                    var key = envVar.Key.ToConfigKey();
                    ProcessedKeyResult? result = null;

                    if (!hasProcessed)
                    {
                        result = ProcessServiceDbOverrideKey(service.Service, envVar, serviceDbOverrideMap, configMap, ref finalEnvVars, ref hasProcessed, ref logs);
                        if (result != null)
                        {
                            processedKeyResult.Add(result);
                        }
                    }

                    if (!hasProcessed)
                    {
                        result = ProcessOverrideKey(envVar, keyOverrideMap, configMap, ref finalEnvVars, ref hasProcessed, ref logs);
                        if (result != null)
                        {
                            processedKeyResult.Add(result);
                        }
                    }

                    if (!hasProcessed)
                    {
                        result = ProcessReferenceKey(envVar, configMap, ref finalEnvVars, ref hasProcessed, ref logs);
                        if (result != null)
                        {
                            processedKeyResult.Add(result);
                        }
                    }

                    if (!hasProcessed)
                    {
                        result = ProcessKeepKey(envVar, ref finalEnvVars, ref hasProcessed, ref logs);
                        if (result != null)
                        {
                            processedKeyResult.Add(result);
                        }
                    }

                }

                var referenceKeys = processedKeyResult.Where(x => x.Kind == ProcessedKeyResult.ProcessKind.Reference);
                var overrideKeys = processedKeyResult.Where(x => x.Kind == ProcessedKeyResult.ProcessKind.Override);
                var serviceDbOverrideKeys = processedKeyResult.Where(x => x.Kind == ProcessedKeyResult.ProcessKind.DbOverride);
                var keepKeys = processedKeyResult.Where(x => x.Kind == ProcessedKeyResult.ProcessKind.Keep);

                logs.AppendLine();

                logs.AppendLine($"==========ExportEnv==========");
                logs.AppendLine($"# keys loaded: {processedKeyResult.Count}");
                logs.AppendLine("==========");
                logs.AppendLine();

                logs.AppendLine($"=========={nameof(serviceDbOverrideKeys)}: {serviceDbOverrideKeys.Count()} ==========");
                foreach (var referenceKey in serviceDbOverrideKeys)
                {
                    logs.AppendLine(referenceKey.ToString());
                }
                logs.AppendLine("==========");
                logs.AppendLine();

                logs.AppendLine($"=========={nameof(overrideKeys)}: {overrideKeys.Count()} ==========");
                foreach (var referenceKey in overrideKeys)
                {
                    logs.AppendLine(referenceKey.ToString());
                }
                logs.AppendLine("==========");
                logs.AppendLine();

                logs.AppendLine($"=========={nameof(referenceKeys)} : {referenceKeys.Count()}==========");
                foreach (var referenceKey in referenceKeys)
                {
                    logs.AppendLine(referenceKey.ToString());
                }
                logs.AppendLine("==========");
                logs.AppendLine();

                logs.AppendLine($"=========={nameof(keepKeys)}: {keepKeys.Count()} ==========");
                foreach (var referenceKey in keepKeys)
                {
                    logs.AppendLine(referenceKey.ToString());
                }
                logs.AppendLine("==========");
                logs.AppendLine();

                using StreamWriter writer = new(Path.Combine(service.Folder, $"log.{DateTime.Now:ddMMyyyyhhmm}"));
                writer.Write(logs.ToString());

                var exportStringBuilder = new StringBuilder();

                foreach (var item in processedKeyResult)
                {
                    exportStringBuilder.AppendLine($"{item.Key.ToExportKey()}={item.NewKey}");
                }

                using StreamWriter exportWriter = new(Path.Combine(service.Folder, $"{service.Config}.{DateTime.Now:ddMMyyyyhhmm}"));
                exportWriter.Write(exportStringBuilder.ToString());
            }
        }

        private static void LoadSvcMap(ref Dictionary<string, ServiceMap> serviceMap)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using var reader = new StreamReader(Path.Combine(Constants.Workspace, "svc-map.csv"));
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<ServiceMap>();
            foreach (var record in records)
            {
                if (!string.IsNullOrEmpty(record.Service) && !string.IsNullOrEmpty(record.Config) && !string.IsNullOrEmpty(record.Folder))
                {
                    serviceMap[record.Service] = record;
                }
                break;
            }
            var folders = Directory.GetDirectories(Constants.Workspace);
            foreach (var folder in folders.Where(x => x.EndsWith("k8s")))
            {
                var serviceName = new DirectoryInfo(folder).Name.ToServiceName();
                if (!serviceMap.ContainsKey(serviceName))
                {
                    serviceMap.Add(serviceName, new ServiceMap()
                    {
                        Service = serviceName,
                        Config = Constants.Service.Default.Config,
                        Folder = folder
                    });
                }
            }
        }

        private static void LoadConfigMap(ref Dictionary<string, string> map)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using var reader = new StreamReader(Path.Combine(Constants.Workspace, "_1.config-map.csv"));
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<ConfigMap>();
            foreach (var record in records)
            {
                if (!map.ContainsKey(record.Key))
                {
                    map[record.Key] = record?.Value ?? string.Empty;
                }
            }
        }

        private static void LoadKeyOverrideMap(ref Dictionary<string, string> map)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using var reader = new StreamReader(Path.Combine(Constants.Workspace, "_2.key-override-map.csv"));
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<KeyOverrideMap>();
            foreach (var record in records)
            {
                if (!map.ContainsKey(record.Key))
                {
                    map[record.Key] = record?.Value ?? string.Empty;
                }
            }
        }

        private static void LoadServiceDbOverrideMap(ref Dictionary<string, List<ServiceDbOverrideMap>> map)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using var reader = new StreamReader(Path.Combine(Constants.Workspace, "_3.svc-db-override-map.csv"));
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<ServiceDbOverrideMap>();
            foreach (var record in records)
            {
                if (!map.ContainsKey(record.Service))
                {
                    map[record.Service] = [new ServiceDbOverrideMap() { Key = record.Key, Service = record.Service, Value = record.Value }];
                }
                else
                {
                    map[record.Service].Add(new ServiceDbOverrideMap() { Key = record.Key, Service = record.Service, Value = record.Value });
                }
            }
        }

        private static IDictionary<string, string> LoadEnv(ServiceMap map, ref StringBuilder logs)
        {
            logs.AppendLine($"=========={nameof(LoadEnv)}==========");
            var envVars = DotEnv.Fluent().WithEnvFiles(Path.Combine(map.Folder, map.Config)).Read();
            logs.AppendLine($"# keys loaded: {envVars.Count}");
            logs.AppendLine("==========");
            return envVars;
        }

        private static ProcessedKeyResult? ProcessReferenceKey(KeyValuePair<string, string> kv, Dictionary<string, string> configMap, ref Dictionary<string, string> finalEnvVars, ref bool hasProcessed, ref StringBuilder logs)
        {
            var configKey = kv.Key.ToConfigKey();

            if (!configMap.ContainsKey(configKey.ToEnvKey()))
            {
                return null;
            }

            finalEnvVars[kv.Key] = configKey.ToEnvKey();
            hasProcessed = true;
            return new ProcessedKeyResult()
            {
                Key = kv.Key.ToConfigKey(),
                NewKey = configKey.ToEnvKey(),
                Kind = ProcessedKeyResult.ProcessKind.Reference,
                Type = ProcessedKeyResult.ProcessType.None,
                OldValue = kv.Value,
                NewValue = configMap.ContainsKey(configKey.ToEnvKey()) ? configMap[configKey.ToEnvKey()] : string.Empty
            };
        }

        private static ProcessedKeyResult? ProcessOverrideKey(KeyValuePair<string, string> kv, Dictionary<string, string> overrideKey, Dictionary<string, string> configMap, ref Dictionary<string, string> finalEnvVars, ref bool hasProcessed, ref StringBuilder logs)
        {
            var configKey = kv.Key.ToConfigKey();
            var map = string.Empty;
            if (overrideKey.ContainsKey(configKey))
            {
                map = overrideKey[configKey];
            }
            if (string.IsNullOrEmpty(map))
            {
                return null;
            }
            finalEnvVars[kv.Key] = map;
            hasProcessed = true;
            return new ProcessedKeyResult()
            {
                Key = kv.Key.ToConfigKey(),
                NewKey = map,
                Kind = ProcessedKeyResult.ProcessKind.Override,
                Type = ProcessedKeyResult.ProcessType.None,
                OldValue = kv.Value,
                NewValue = configMap.ContainsKey(map) ? configMap[map] : string.Empty
            };
        }

        private static ProcessedKeyResult? ProcessServiceDbOverrideKey(string serviceName, KeyValuePair<string, string> kv, Dictionary<string, List<ServiceDbOverrideMap>> serviceDbOverride, Dictionary<string, string> configMap, ref Dictionary<string, string> finalEnvVars, ref bool hasProcessed, ref StringBuilder logs)
        {
            if (!serviceDbOverride.ContainsKey(serviceName))
            {
                return null;
            }
            var configKey = kv.Key.ToConfigKey();
            var map = string.Empty;
            var serviceMap = serviceDbOverride[serviceName];
            if (serviceMap == null)
            {
                return null;
            }
            var keyMap = serviceMap.FirstOrDefault(x => x.Key == configKey);
            if (keyMap == null)
            {
                return null;
            }

            map = keyMap.Value;

            if (string.IsNullOrEmpty(map))
            {
                return null;
            }
            finalEnvVars[kv.Key] = map;
            hasProcessed = true;
            return new ProcessedKeyResult()
            {
                Key = kv.Key.ToConfigKey(),
                NewKey = map,
                Kind = ProcessedKeyResult.ProcessKind.DbOverride,
                Type = ProcessedKeyResult.ProcessType.None,
                OldValue = kv.Value,
                NewValue = configMap.ContainsKey(map) ? configMap[map] : string.Empty
            };
        }

        private static ProcessedKeyResult? ProcessKeepKey(KeyValuePair<string, string> kv, ref Dictionary<string, string> finalEnvVars, ref bool hasProcessed, ref StringBuilder logs)
        {
            var map = kv.Value;
            finalEnvVars[kv.Key] = map;
            hasProcessed = true;
            return new ProcessedKeyResult()
            {
                Key = kv.Key.ToConfigKey(),
                NewKey = kv.Key.ToConfigKey(),
                Kind = ProcessedKeyResult.ProcessKind.Keep,
                Type = ProcessedKeyResult.ProcessType.None,
                OldValue = kv.Value,
                NewValue = map
            };
        }

    }
}