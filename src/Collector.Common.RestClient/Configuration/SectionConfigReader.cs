#if NETSTANDARD2_0
namespace Collector.Common.RestClient.Configuration
{
    using System;

    using Collector.Common.RestClient.Exceptions;

    using Microsoft.Extensions.Configuration;

    public class SectionConfigReader : IConfigReader
    {
        private readonly IConfigurationSection _configurationSection;
        private readonly IConfigurationSection _subConfigurationSection;

        public SectionConfigReader(IConfigurationSection configurationSection, IConfigurationSection subConfigurationSection)
        {
            _configurationSection = configurationSection;
            _subConfigurationSection = subConfigurationSection;
        }

        public string GetString(string key, bool onlyFromSubSection = true)
        {
            return _subConfigurationSection.GetValue<string>(key)
                   ?? (onlyFromSubSection ? null : _configurationSection.GetValue<string>(key));
        }

        public string GetAndEnsureString(string key, bool onlyFromSubSection = true)
        {
            var fromAppSettingsKey = GetString(key, onlyFromSubSection);

            if (fromAppSettingsKey == null)
                throw new RestClientConfigurationException($"Could not find '{key}' for {_subConfigurationSection.Key} in config");

            return fromAppSettingsKey;
        }

        public TimeSpan? GetTimeSpan(string key)
        {
            return _subConfigurationSection.GetValue<TimeSpan?>(key)
                   ?? _configurationSection.GetValue<TimeSpan?>(key);
        }
    }
}
#endif