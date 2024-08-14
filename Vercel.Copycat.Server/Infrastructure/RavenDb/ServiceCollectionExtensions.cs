using System.Security.Cryptography.X509Certificates;
using Raven.Client.Documents;

namespace Vercel.Copycat.Server.Infrastructure.RavenDb;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRavenDb(this IServiceCollection services, IConfiguration config)
    {
        var (dbSettings, cert) = config.GetRavenDbSettings();

        var store = new DocumentStore
        {
            Urls = dbSettings.Urls,
            Database = dbSettings.DatabaseName,
            Certificate = cert,

        }.Initialize();

        return services.AddSingleton(_ => store);
    }

    private static (RavenSettings, X509Certificate2) GetRavenDbSettings(this IConfiguration configuration)
    {
        var dbSettings = configuration.GetSection(nameof(RavenSettings)).Get<RavenSettings>();

        X509Certificate2 certificate;
        if (!string.IsNullOrWhiteSpace(dbSettings!.Thumbprint))
        {
            certificate = LoadByThumbprint(dbSettings.Thumbprint);
        }
        else if (!string.IsNullOrWhiteSpace(dbSettings.CertContent))
        {
            var bytes = Convert.FromBase64String(dbSettings.CertContent);
            certificate = new X509Certificate2(bytes);
        }
        else
        {
            certificate = (!string.IsNullOrEmpty(dbSettings.CertPath)
                ? new X509Certificate2(dbSettings.CertPath, dbSettings.CertPass)
                : null)!;
        }

        return (dbSettings, certificate);
    }

    private static X509Certificate2 LoadByThumbprint(string thumbprint)
    {
        using var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        certStore.Open(OpenFlags.ReadOnly);

        var cert = certStore.Certificates.OfType<X509Certificate2>()
            .FirstOrDefault(x => x.Thumbprint == thumbprint);

        return cert;
    }
}

public class RavenSettings
{
    public string[] Urls { get; set; } = [];
    public string DatabaseName { get; set; } = string.Empty;
    public string? CertPath { get; set; }
    public string? CertPass { get; set; }
    public string? Thumbprint { get; set; }
    public string? SecondaryThumbprint { get; set; }
    public string? CertContent { get; set; }
    public string[] Profile { get; set; } = [];

    public static (RavenSettings, X509Certificate2) FromConfig(IConfiguration configuration, string sectionName = null!)
    {
        var dbSettings = new RavenSettings();
        configuration.Bind(sectionName ?? nameof(RavenSettings), dbSettings);
        var certificate = !string.IsNullOrEmpty(dbSettings?.CertPath)
            ? new X509Certificate2(dbSettings.CertPath, dbSettings.CertPass)
            : null;
        return (dbSettings, certificate);
    }
}