namespace Collector.Common.RestClient
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    internal class ConfigReader
    {
        public static T GetValueFromAppSettingsKey<T>(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var str = ConfigurationManager.AppSettings[key];
            if (!String.IsNullOrWhiteSpace(str))
            {
                str = str.Replace("&amp;", "&");
            }
            else
            {
                return default(T);
            }

            return (T)Convert.ChangeType(str, typeof(T));
        }

        public static T GetAndEnsureValueFromAppSettingsKey<T>(string key)
        {
            var fromAppSettingsKey = GetValueFromAppSettingsKey<T>(key);

            if (fromAppSettingsKey != null)
            {
                return fromAppSettingsKey;
            }

            throw new KeyNotFoundException($"The key '{key}' is not found in the config file.");
        }
    }
}