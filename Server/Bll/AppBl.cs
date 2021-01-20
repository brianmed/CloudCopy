using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

using CloudCopy.Server.DbContexts;
using CloudCopy.Server.Entities;

namespace CloudCopy.Server.Bll
{
    class CallerEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            int skip = 3;

            while (true)
            {
                StackFrame stack = new StackFrame(skip, true);

                if (!stack.HasMethod())
                {
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue("<unknown method>")));

                    return;
                }

                MethodBase method = stack.GetMethod();

                bool stopHere = method.DeclaringType.Assembly.FullName.Contains(nameof(CloudCopy))
                    && !method.DeclaringType.FullName.Contains($"{nameof(CloudCopy)}DbContext");

                if (stopHere)
                {
                    string caller = $"{stack.GetFileName()}: {stack.GetFileLineNumber()}";

                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller)));

                    return;
                }

                skip++;
            }
        }
    }

    static class LoggerCallerEnrichmentConfiguration
    {
        public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With<CallerEnricher>();
        }
    }

    public class AppBl
    {
        public static ILogger LogApp { get; internal set;}

        public static ILogger LogSql { get; internal set;}

        public static void InitLogging()
        {
            string outputTemplate;

            string appLogging = Regex.Replace((Environment.GetEnvironmentVariable($"{nameof(CloudCopy).ToUpper()}_APP_LOGGING")
                ?? Environment.GetEnvironmentVariable($"{nameof(CloudCopy).ToUpper()}_DEFAULT_LOGGING")
                ?? String.Empty),
                "^$", "None");

            if (appLogging == "None") {
                LogApp = Serilog.Core.Logger.None;
            } else {
                string appLogFile = Path.Combine(Program.AppCfg.LogDirectory, $"{nameof(CloudCopy)}-app.log");
                Console.WriteLine($"Initializing App Log: {appLogging} {appLogFile}");

                LoggerConfiguration logConfiguration = new LoggerConfiguration();

                try
                {
                    logConfiguration = (LoggerConfiguration)(logConfiguration
                        .MinimumLevel.GetType()
                        .GetMethod(Enum.Parse<LogEventLevel>(appLogging).ToString())
                        .Invoke(logConfiguration.MinimumLevel, null));
                }
                catch (Exception ex)
                {
                    throw new Exception($"Please use a minimum logging level of None, Verbose, Debug, Information, Warning, Error, Fatal: {ex.Message}");
                }

                if (appLogging == "Debug" || appLogging == "Verbose") {
                    outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} (near {Caller}){NewLine}{Exception}";

                    logConfiguration = logConfiguration
                        .Enrich.WithCaller();
                } else {
                    outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message}{NewLine}{Exception}";
                }

                logConfiguration = logConfiguration
                    .Enrich.FromLogContext();

                LogApp = logConfiguration
                    .Enrich.FromLogContext()
                    .WriteTo.File(appLogFile,
                        outputTemplate: outputTemplate,
                        fileSizeLimitBytes: 1024 * 1024 * 256,
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: 4)
                    .CreateLogger();

                LogApp.Information("Started App Logging");
            }

            string sqlLogging = Regex.Replace((Environment.GetEnvironmentVariable($"{nameof(CloudCopy).ToUpper()}_SQL_LOGGING")
                ?? Environment.GetEnvironmentVariable($"{nameof(CloudCopy).ToUpper()}_DEFAULT_LOGGING")
                ?? String.Empty),
                "^$", "None");

            if (sqlLogging == "None") {
                LogSql = Serilog.Core.Logger.None;
            } else {
                string appSqlFile = Path.Combine(Program.AppCfg.LogDirectory, $"{nameof(CloudCopy)}-sql.log");
                Console.WriteLine($"Initializing SQL Log: {sqlLogging} {appSqlFile}");

                LoggerConfiguration logConfiguration = new LoggerConfiguration();

                try
                {
                    logConfiguration = (LoggerConfiguration)(logConfiguration
                        .MinimumLevel.GetType()
                        .GetMethod(Enum.Parse<LogEventLevel>(sqlLogging).ToString())
                        .Invoke(logConfiguration.MinimumLevel, null));
                }
                catch (Exception ex)
                {
                    throw new Exception($"Please use a minimum logging level of None, Verbose, Debug, Information, Warning, Error, Fatal: {ex.Message}");
                }

                if (sqlLogging == "Debug" || sqlLogging == "Verbose") {
                    outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} (near {Caller}){NewLine}{Exception}";

                    logConfiguration = logConfiguration
                        .Enrich.WithCaller();
                } else {
                    outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message}{NewLine}{Exception}";
                }

                LogSql = logConfiguration
                    .Enrich.FromLogContext()
                    .WriteTo.File(appSqlFile,
                        outputTemplate: outputTemplate,
                        fileSizeLimitBytes: 1024 * 1024 * 256,
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: 4)
                    .CreateLogger();

                LogSql.Information("Started SQL Logging");
            }
        }

        async public static Task InitAsync(IHost host)
        {
            if (!AppDirectoriesBl.IsProvisioned()) {
                AppDirectoriesBl.Provision();
            }

            InitLogging();

            using (CloudCopyDbContext appDbContext = new CloudCopyDbContext())
            {
                appDbContext.Database.Migrate();
            }

            if (!await IsAppProvisioned()) {
                await CreateAppRecordAsync();
            }

            if (!await Program.OptionsBl.HasRecord()) {
                await Program.OptionsBl.CreateRecordAsync();
            }

            await Program.OptionsBl.LoadRecord();
        }

        async private static Task CreateAppRecordAsync()
        {
            using (CloudCopyDbContext appDbContext = new CloudCopyDbContext())
            {
                await appDbContext.App.AddAsync(new AppEntity
                {
                    AppEntityId = 1,
                    JwtSecret = System.Guid.NewGuid().ToString()
                });

                await appDbContext.SaveChangesAsync();
            }
        }

        async private static Task<bool> IsAppProvisioned()
        {
            using (CloudCopyDbContext appDbContext = new CloudCopyDbContext())
            {
                return await appDbContext.App.CountAsync() > 0;
            }
        }

        public static string CreateTempFileWithExtension(string extension)
        {
            string file = string.Empty;

            do
            {
                file = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid().ToString()}.{extension}");
            } while (File.Exists(file));

            return file;
        }
    }
}
