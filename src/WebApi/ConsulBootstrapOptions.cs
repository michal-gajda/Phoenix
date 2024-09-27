namespace Phoenix.WebApi;

public sealed class ConsulBootstrapOptions
{
    public static readonly string SECTION_NAME = "Consul";
    public Uri Address { get; set; } = new("http://localhost:8500");
    public string Environment { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
}
