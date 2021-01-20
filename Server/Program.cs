using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Events;

using CloudCopy.Server.Bll;

namespace CloudCopy.Server
{
    public class Program
    {
        static public AppCfg AppCfg = new AppCfg();

        static public OptionsBl OptionsBl = new OptionsBl();        

        async public static Task Main(string[] args)
        {
            AppCfg.ParseOptions(args);

            IHost host = CreateHostBuilder(args).Build();

            await AppBl.InitAsync(host);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseSerilog();
                });
    }
}
