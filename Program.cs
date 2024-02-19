using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

using MIPService;
using Serilog;

namespace MIPService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .WriteTo.File(@"C:\\Logs\MIP\LogFile.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            //CreateHostBuilder(args).Build().Run();

            try
            {
                Log.Information("Starting service");
                CreateHostBuilder(args).Build().Run();
                return;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Problem starting the service");
                return;
            }
            finally
            {
                Log.CloseAndFlush();
            }

            //CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args).UseWindowsService()
               .ConfigureServices((hostContext, services) =>
               {
                   IConfiguration configuration = hostContext.Configuration;
                   AppSettings options = configuration.GetSection("AppSettings").Get<AppSettings>();
                   services.AddSingleton(options);
                   services.AddHostedService<Worker>();
               });
    }
}
