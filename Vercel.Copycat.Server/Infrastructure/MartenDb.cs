using Marten;
using Weasel.Core;

namespace Vercel.Copycat.Server.Infrastructure;

public static class MartenDb
{
    public static IServiceCollection AddMartenDb(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        services
            .AddMarten(options =>
            {
                options.Connection(config.GetConnectionString("postgresql")!);
                if (env.IsDevelopment())
                    options.AutoCreateSchemaObjects = AutoCreate.All;
            });
        return services;
    }
}