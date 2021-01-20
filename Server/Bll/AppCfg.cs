using System;
using System.IO;
using System.Linq;

using Microsoft.Extensions.PlatformAbstractions;

using Mono.Options;

using CloudCopy.Server.DbContexts;

namespace CloudCopy.Server.Bll
{
    public class AppCfg
    {
        public string BaseDirectory { get; set; } = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), $".{nameof(CloudCopy)}");

        public string FilesDirectory
        {
            get
            {
                return Path.Combine(BaseDirectory, "Files");
            }
        }

        public string AppDbDirectory
        {
            get
            {
                return Path.Combine(BaseDirectory, "AppDb");
            }
        }

        public string AppDbFile
        {
            get
            {
                return Path.Combine(AppDbDirectory, $"{nameof(CloudCopy)}.sqlite");
            }
        }

        public string AppDbConnectionString
        {
            get
            {
                return $"Data Source={AppDbFile};";
            }
        }

            
        public string JwtSecret
        {
            get
            {
                using (CloudCopyDbContext appDbContext = new CloudCopyDbContext())
                {
                    return appDbContext.App.Single().JwtSecret;
                }
            }
        }

        public string JwtIssuer
        {
            get
            {
                using (CloudCopyDbContext appDbContext = new CloudCopyDbContext())
                {
                    return appDbContext.AdminOptions.SingleOrDefault()?.SiteUrl + "/";
                }
            }
        }

        public string LogDirectory
        {
            get
            {
                return Path.Combine(BaseDirectory, "Log");
            }
        }

        public void ParseOptions(string[] args)
        {
            bool helpRequested = false;

            OptionSet optionSet = new OptionSet { 
                { "baseDirectory=", "App BaseDirectory", baseDirectory => BaseDirectory = baseDirectory },                

                { "help", "Show this message and exit", help => helpRequested = help != null },
            };            

            try {
                optionSet.Parse(args);

                if (helpRequested) {
                    Help(optionSet);

                    Environment.Exit(0);
                }
            } catch (OptionException e) {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"Try '{PlatformServices.Default.Application.ApplicationName} --help' for more information.");

                Environment.Exit(1);
            }   
        }

        public void Help(OptionSet optionSet)
        {
            Console.WriteLine($"Usage: {PlatformServices.Default.Application.ApplicationName} [OPTIONS]");

            Console.WriteLine("Options:");
            optionSet.WriteOptionDescriptions(Console.Out);
        }
    }
}

