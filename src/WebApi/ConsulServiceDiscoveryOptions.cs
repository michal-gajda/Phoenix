namespace Phoenix.WebApi;

public sealed class ConsulServiceDiscoveryOptions
{
    public static readonly string SECTION_NAME = "Consul";

    public string Environment { get; set; } = string.Empty;
    public string ServiceAddress { get; set; } = "localhost";
    public string? ServiceHealthCheckAddress { get; set; } = default;
    public string ServiceName { get; set; } = string.Empty;
    public int ServicePort { get; set; } = default;
    public string[] ServiceTags { get; set; } = default!;
}
