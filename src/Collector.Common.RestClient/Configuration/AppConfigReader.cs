namespace Collector.Common.RestClient.Configuration
{
    using System;
    using System.Configuration;

    using Collector.Common.RestClient.Exceptions;

    internal class AppConfigReader : IConfigReader
    {
        private readonly string _contractKey;

        public AppConfigReader(string contractKey)
        {
            _contractKey = contractKey;
        }

        public string GetString(string key, bool onlyFromSubSection = false)
        {
            return ReadCompositeKeyAsString($"{_contractKey}.{key}")
                   ?? (onlyFromSubSection ? null : ReadCompositeKeyAsString(key));
        }

        public string GetAndEnsureString(string key, bool onlyFromSubSection = false)
        {
            var fromAppSettingsKey = GetString(key, onlyFromSubSection);

            if (fromAppSettingsKey == null)
                throw new RestClientConfigurationException($"Could not find '{key}' for {_contractKey} in config");

            return fromAppSettingsKey;
        }

        public TimeSpan? GetTimeSpan(string key)
        {
            return ReadCompositeKeyAsTimeSpan($"{_contractKey}.{key}")
                   ?? ReadCompositeKeyAsTimeSpan($"{key}");
        }

        private string ReadCompositeKeyAsString(string compositeKey)
        {
            if (string.IsNullOrWhiteSpace(compositeKey))
                throw new ArgumentNullException(nameof(compositeKey));

            var str = ConfigurationManager.AppSettings[$"RestClient:{compositeKey}"];

            if (string.IsNullOrWhiteSpace(str))
                return null;

            return str.Replace("&amp;", "&");
        }

        private TimeSpan? ReadCompositeKeyAsTimeSpan(string compositeKey)
        {
            var value = ReadCompositeKeyAsString(compositeKey);
            if (value == null)
                return null;

            TimeSpan timespan;
            if (!TimeSpan.TryParse(value, out timespan))
                throw new RestClientConfigurationException($"'RestClient:{compositeKey}' must be parseable to a timespan");

            return timespan;
        }
    }
}