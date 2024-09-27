namespace Phoenix.WebApi;

using System.Diagnostics;
using System.Net;
using System.Reflection;
using Consul;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public static class Program
{
    private static readonly FileVersionInfo FILE_VERSION_INFO = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
    private static string ServiceName => FILE_VERSION_INFO.ProductName!;
    private static string ServiceVersion => FILE_VERSION_INFO.ProductVersion!;

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddConsul();
        builder.Services.AddConsul(builder.Configuration);

        builder.WebHost.ConfigureKestrel((_, server) =>
        {
            var options = builder.Configuration.GetSection(ConsulServiceDiscoveryOptions.SECTION_NAME).Get<ConsulServiceDiscoveryOptions>()!;

            server.Listen(IPAddress.Any,
                options.ServicePort,
                listen =>
                {
                    listen.UseConnectionLogging();
                });
        });

        builder.Logging.AddOpenTelemetry(cfg =>
        {
            cfg.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName, serviceVersion: ServiceVersion)).AddOtlpExporter();
        });
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(ServiceName, serviceVersion: ServiceVersion))
            .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddOtlpExporter())
            .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation().AddOtlpExporter());

        builder.Services.AddHealthChecks();

        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseHealthChecks("/health");

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var consulClient = app.Services.GetRequiredService<IConsulClient>();
            var options = app.Services.GetRequiredService<IOptions<ConsulServiceDiscoveryOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.Environment))
            {
                options.Environment = $"{app.Environment.EnvironmentName}";
            }

            var serviceId = $"{options.Environment}_{options.ServiceName}_{options.ServiceAddress}:{options.ServicePort}";

            var registration = new AgentServiceRegistration
            {
                ID = serviceId,
                Name = options.ServiceName,
                Address = options.ServiceAddress,
                Port = options.ServicePort,
                Tags = options.ServiceTags,
            };

            consulClient.Agent.ServiceDeregister(registration.ID).Wait();
            consulClient.Agent.ServiceRegister(registration).Wait();
        });

        app.Lifetime.ApplicationStopped.Register(() =>
        {
            var consulClient = app.Services.GetRequiredService<IConsulClient>();
            var options = app.Services.GetRequiredService<IOptions<ConsulServiceDiscoveryOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.Environment))
            {
                options.Environment = $"{app.Environment.EnvironmentName}";
            }

            var serviceId = $"{options.Environment}_{options.ServiceName}_{options.ServiceAddress}:{options.ServicePort}";
            consulClient.Agent.ServiceDeregister(serviceId).Wait();
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();


        app.MapControllers();

        await app.RunAsync();
    }
}
