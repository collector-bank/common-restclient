namespace Collector.Common.RestClient.Configuration
{
    using System;

    internal interface IConfigReader
    {
        string GetString(string key, bool onlyFromSubSection = false);

        string GetAndEnsureString(string key, bool onlyFromSubSection = false);

        TimeSpan? GetTimeSpan(string key);
    }
}