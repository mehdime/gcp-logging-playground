using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using Serilog.Formatting.Stackdriver;

namespace LoggingDemo
{
    public class Program
    {
        private const string DefaultPort = "6100";
        private const string DefaultHostname = "*";
        
        public static void Main(string[] args)
        {
            /*
             * Guide for happy logging when using Stackdriver:
             *
             * 1. Write logs to the standard output.
             * 2. Write structured logs in JSON format.
             * 3. Use those properties in your logs: 'timestamp', 
             * 'severity', 'message' and 'exception'. Your life 
             * will be a lot easier.
             */
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                // Exclude debug logs coming from the ASP.NET runtime 
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(new StackdriverJsonFormatter())
                .CreateLogger();

            Log.Logger.Information(".NET Core Stackdriver logging demo starting...");

            /*
             * See the HomeController class for more logging examples. 
             */

            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var port = Environment.GetEnvironmentVariable("PORT") ?? DefaultPort;
            var hostname = Environment.GetEnvironmentVariable("HOSTNAME") ?? DefaultHostname;
            
            return WebHost.CreateDefaultBuilder(args)
                .UseUrls($"http://{hostname}:{port}")
                // Redirect the ASP.NET runtime logs to Serilog
                .UseSerilog()
                .UseStartup<Startup>();
        }
    }
}