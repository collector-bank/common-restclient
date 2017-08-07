[![Build status](https://ci.appveyor.com/api/projects/status/l8bm6mcxhgvu5046/branch/master?svg=true)](https://ci.appveyor.com/project/HoudiniCollector/common-restclient/branch/master)

# Collector Common RestClient

## Configuration through config section
startup.cs:
```csharp
var section = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build()
				.GetSection("RestClient");

var restApiClient = new ApiClientBuilder()
				.ConfigureFromConfigSection(section)
				.Build(); // The client returned here should be used/registered as a singleton.
```
appsettings.json:
```json
{
  "RestClient": {
    "ClientId": "my_client_id",
    "ClientSecret": "my_client_secret",
    "Authentication": "oauth2",
    "Issuer": "my_issuer.com",

    "Apis": {
      "MyApi": {
        "Audience": "https://my-api.com",
        "BaseUrl": "https://my-api.com",
        "Timeout": "00:00:30"
      }
    }
  }
}
```
Note that it is valid to configure anything on the api level, so if you add a property "ClientId" to the "MyApi" object then that client id will be used for MyApi, but for all other apis the global one will be used.
## Configuration through app/web.config
startup.cs:
```csharp
var restApiClient = new ApiClientBuilder()
                .ConfigureFromAppSettings()
                .Build(); // The client returned here should be used/registered as a singleton.
```
app.config:
```xml
<configuration>
  <appSettings>
    <add key="RestClient:ClientId" value="my_client_id"/>
    <add key="RestClient:ClientSecret" value="my_client_secret"/>
    <add key="RestClient:Authentication" value="oauth2"/>
    <add key="RestClient:Issuer" value="my_issuer.com"/>
    
    <add key="RestClient:MyApi.Audience" value="https://my-api.com"/>
    <add key="RestClient:MyApi.BaseUrl" value="https://my-api.com"/>
    <add key="RestClient:MyApi.Timeout" value="00:00:30"/>
  </appSettings>
</configuration>
```
Note that it is valid to configure anything on the api level, so if you add a property "RestClient:MyApi.ClientId" then that client id will be used for MyApi, but for all other apis the global one will be used.
## Other configuration options
It is also highly recommended to configure a serilog logger and context management:

```csharp
var provider = new ApiClientBuilder()
                .WithLogger(Log.Logger) //Serilog
                .WithContextFunction(() => CorrelationState.GetCurrentCorrelationId()?.ToString()) // Collector.Common.Correlation
                .Build();
```

