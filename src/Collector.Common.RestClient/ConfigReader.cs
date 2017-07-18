namespace Collector.Common.RestClient
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    internal class ConfigReader
    {
        public static string GetValueFromAppSettingsKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var str = ConfigurationManager.AppSettings[$"RestClient:{key}"];

            if (string.IsNullOrWhiteSpace(str))
                return null;

            return str.Replace("&amp;", "&");
        }

        public static string GetAndEnsureValueFromAppSettingsKey(string key)
        {
            var fromAppSettingsKey = GetValueFromAppSettingsKey(key);

            if (fromAppSettingsKey == null)
                throw new KeyNotFoundException($"Could not find {key} in config, configure a value for key 'RestClient:{key}'");

            return fromAppSettingsKey;
        }

        public static string GetValueFromAppSettingsKey(string contractKey, string key)
        {
            return GetValueFromAppSettingsKey($"{contractKey}.{key}")
                   ?? GetValueFromAppSettingsKey(key);
        }

        public static string GetAndEnsureValueFromAppSettingsKey(string contractKey, string key)
        {
            var fromAppSettingsKey = GetValueFromAppSettingsKey(contractKey, key);

            if (fromAppSettingsKey == null)
                throw new KeyNotFoundException($"Could not find {key} in config, either configure a value for key 'RestClient:{key}' or key 'RestClient:{contractKey}.{key}'");

            return fromAppSettingsKey;
        }
    }
}