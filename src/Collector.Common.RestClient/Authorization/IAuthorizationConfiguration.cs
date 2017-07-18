namespace Collector.Common.RestClient.Authorization
{
    using Serilog;

    public interface IAuthorizationConfiguration
    {
        IAuthorizationHeaderFactory CreateFactory(ILogger logger);
    }
}