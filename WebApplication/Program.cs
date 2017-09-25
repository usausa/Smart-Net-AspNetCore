namespace WebApplication
{
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    public class Program
    {
        public static void Main(string[] args)
        {
            // Disable ApplicationInsight
            TelemetryConfiguration.Active.DisableTelemetry = true;

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .CaptureStartupErrors(true) // Capture Startup Errors
                .UseStartup<Startup>()
                .Build();
    }
}
