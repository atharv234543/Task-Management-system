using Serilog;

namespace WpfApp1.Logging
{
    public static class SerilogConfig
    {
        public static void Configure()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("Logs/app-.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
}
