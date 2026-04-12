using Microsoft.EntityFrameworkCore;
using PureDelivery.Common.Configuration.Services;
using PureDelivery.CourierService.Core.Configuration;
using PureDelivery.CourierService.Infrastructure.Data;

namespace PureDelivery.CourierService.Helpers
{
    public static class IoCHelper
    {
        public static async Task ConfigureDatabaseAsync(WebApplicationBuilder builder)
        {
            var dbConfig = await LoadDatabaseConfigurationAsync(builder);

            builder.Services.AddDbContext<CourierDbContext>(options =>
            {
                ConfigureSqlServer(options, dbConfig);
                ConfigureLogging(options, dbConfig);
            });

            LogDatabaseConfiguration(dbConfig);
        }

        static async Task<CourierServiceConfig> LoadDatabaseConfigurationAsync(WebApplicationBuilder builder)
        {
            using var tempServiceProvider = builder.Services.BuildServiceProvider();

            var configProvider = tempServiceProvider.GetRequiredService<ICustomConfigurationProvider>();
            return await configProvider.GetConfigurationAsync<CourierServiceConfig>("CourierService");
        }

        static void ConfigureSqlServer(DbContextOptionsBuilder options, CourierServiceConfig config)
        {
            options.UseSqlServer(config.ConnectionString, sqlOptions =>
            {
                if (config.Database.EnableRetryOnFailure)
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: config.Database.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(config.Database.MaxRetryDelay),
                        errorNumbersToAdd: null);
                }

                sqlOptions.CommandTimeout(config.Database.CommandTimeout);
            });
        }

        static void ConfigureLogging(DbContextOptionsBuilder options, CourierServiceConfig config)
        {
            if (config.Database.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        }

        static void LogDatabaseConfiguration(CourierServiceConfig config)
        {
            Console.WriteLine($"   Courier Service Database Configuration:");
            Console.WriteLine($"   Retry Enabled: {config.Database.EnableRetryOnFailure}");
            Console.WriteLine($"   Max Retry Count: {config.Database.MaxRetryCount}");
            Console.WriteLine($"   Max Retry Delay: {config.Database.MaxRetryDelay}s");
            Console.WriteLine($"   Command Timeout: {config.Database.CommandTimeout}s");
            Console.WriteLine($"   Sensitive Logging: {config.Database.EnableSensitiveDataLogging}");
        }
    }
}
