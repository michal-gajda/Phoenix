namespace Phoenix.WebApi;

using Consul;
using Winton.Extensions.Configuration.Consul;

public static class ConfigurationBuilderExtensions
{
    public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration)
    {
        var consulBootstrapOptions = configuration.GetSection(ConsulBootstrapOptions.SECTION_NAME).Get<ConsulBootstrapOptions>()!;

        services.AddSingleton<IConsulClient, ConsulClient>(_ => new ConsulClient(consulConfig => consulConfig.Address = consulBootstrapOptions.Address));

        services.Configure<ConsulBootstrapOptions>(configuration.GetSection(ConsulBootstrapOptions.SECTION_NAME));
        services.Configure<ConsulServiceDiscoveryOptions>(configuration.GetSection(ConsulServiceDiscoveryOptions.SECTION_NAME));

        return services;
    }

    public static IConfigurationBuilder AddConsul(this IConfigurationBuilder configurationBuilder)
    {
        var configuration = configurationBuilder.Build();
        var consulBootstrapOptions = configuration.GetSection(ConsulBootstrapOptions.SECTION_NAME).Get<ConsulBootstrapOptions>()!;

        var key = BuildKey(consulBootstrapOptions.Environment, consulBootstrapOptions.ServiceName);

        return configurationBuilder.AddConsul(key,
            options =>
            {
                options.ConsulConfigurationOptions = cco => cco.Address = consulBootstrapOptions.Address;
                options.Optional = true;
                options.PollWaitTime = TimeSpan.FromSeconds(5);
                options.ReloadOnChange = true;
            });
    }

    private static string BuildKey(params string[] keys)
    {
        var items = new List<string>();

        foreach (var key in keys)
        {
            if (string.IsNullOrWhiteSpace(key) is false)
            {
                items.Add(key);
            }
        }

        return string.Join("/", items);
    }
}
