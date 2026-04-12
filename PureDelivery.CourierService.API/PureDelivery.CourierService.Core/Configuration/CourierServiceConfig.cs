namespace PureDelivery.CourierService.Core.Configuration
{
    public class CourierServiceConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
        public DatabaseConfig Database { get; set; } = new();
    }

    public class DatabaseConfig
    {
        public bool EnableRetryOnFailure { get; set; } = true;
        public int MaxRetryCount { get; set; } = 3;
        public int MaxRetryDelay { get; set; } = 30;
        public int CommandTimeout { get; set; } = 30;
        public bool EnableSensitiveDataLogging { get; set; } = false;
    }
}
