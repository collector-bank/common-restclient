namespace Collector.Common.RestClient
{
#if NET45
    using System.Configuration;
    using System.Linq;

    using Collector.Common.RestClient.Configuration;

    public class ApiClientBuilder : ApiClientBuilderBase
    {
        /// <summary>
        /// Configure the IRestApiClient using app config values.
        /// </summary>
        public ApiClientBuilder ConfigureFromAppSettings()
        {
            foreach (var baseUrlSetting in ConfigurationManager.AppSettings.AllKeys.Where(k => k.StartsWith("RestClient:")).Where(k => k.EndsWith(".BaseUrl")))
            {
                var contractKey = baseUrlSetting.Split(':').Last().Split('.').First();
                ConfigureContractKey(contractKey, new AppConfigReader(contractKey));
            }

            return this;
        }
    }
#endif

#if NETSTANDARD2_0
    using System.Linq;

    using Collector.Common.RestClient.Configuration;

    using Microsoft.Extensions.Configuration;

    public class ApiClientBuilder : ApiClientBuilderBase
    {
        /// <summary>
        /// Configure the IRestApiClient using app config values.
        /// </summary>
        /// <summary>
        /// Configure the IRestApiClient using a configuration section.
        /// </summary>
        public ApiClientBuilder ConfigureFromConfigSection(IConfigurationSection configurationSection)
        {
            foreach (var subSection in configurationSection.GetSection("Apis").GetChildren())
            {
                ConfigureContractKey(subSection.Key, new SectionConfigReader(configurationSection, subSection));
            }

            return this;
        }
    }
#endif
}