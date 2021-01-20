using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

using Serilog;
using Serilog.Events;

using CloudCopy.Server.DbContexts;
using CloudCopy.Server.Repositories;

namespace CloudCopy.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                SetupWebLog();

                loggingBuilder.AddSerilog(dispose: true);
            });            

            services.AddDbContext<CloudCopyDbContext>();

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Program.AppCfg.BaseDirectory, "DataProtection")));

            services.AddControllersWithViews()
                .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);


            services.AddDistributedMemoryCache();
            
            services.AddSession();            

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;

                    options.Events = new JwtBearerEvents();

                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = Program.AppCfg.JwtIssuer,
                        ValidAudience = Program.AppCfg.JwtIssuer,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Program.AppCfg.JwtSecret))
                    };
            });                      

            services.AddHttpContextAccessor();

            services.AddScoped<IAppRepository, AppRepository>();
            services.AddScoped<ICopiedRepository, CopiedRepository>();
            services.AddScoped<ICopiedService, CopiedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopped.Register(OnStopped);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                appLifetime.StopApplication();

                // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                eventArgs.Cancel = true;
            };

            app.UseForwardedHeaders(new ForwardedHeadersOptions{ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto});

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseMiddleware<JwtInHeaderMiddleware>();

            app.UseStatusCodePages((context) => {
                HttpRequest request = context.HttpContext.Request;
                HttpResponse response = context.HttpContext.Response;

                if (response.StatusCode == (int)HttpStatusCode.Unauthorized && context.HttpContext.Request.Path.Value != "/") {
                    response.Redirect("/");
                }

                return Task.CompletedTask;
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }

        private void SetupWebLog()
        {
            string webLogFile = Path.Combine(Program.AppCfg.LogDirectory, $"{nameof(CloudCopy)}-web.log");

            LoggerConfiguration webLogConfig = new LoggerConfiguration()
                .MinimumLevel.Debug();

            string webLogLevel = Regex.Replace((Environment.GetEnvironmentVariable($"{nameof(CloudCopy).ToUpper()}_WEB_LOGGING")
                ?? Environment.GetEnvironmentVariable($"{nameof(CloudCopy).ToUpper()}_DEFAULT_LOGGING")
                ?? String.Empty),
                "^$", "None");

            ILogger webLog;

            if (webLogLevel == "None") {
                webLog = Serilog.Core.Logger.None;
            } else {
                Console.WriteLine($"Initializing Web Log: {webLogLevel} {webLogFile}");

                try
                {
                    webLogConfig = (LoggerConfiguration)(webLogConfig
                        .MinimumLevel.GetType()
                        .GetMethod(Enum.Parse<LogEventLevel>(webLogLevel).ToString())
                        .Invoke(webLogConfig.MinimumLevel, null));
                }
                catch (Exception ex)
                {
                    throw new Exception($"Please use a minimum logging level of None, Verbose, Debug, Information, Warning, Error, Fatal: {ex.Message}");
                }

                webLog = webLogConfig
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information) 
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("System", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.File(webLogFile, fileSizeLimitBytes: 1024 * 1024 * 256, rollOnFileSizeLimit: true, retainedFileCountLimit: 4)
                    .CreateLogger();

                webLog.Information("Started Web Logging");
            }

            Log.Logger = webLog;
        }

        private void OnStarted()
        {
            Console.WriteLine("CloudCopy Web Server Started...");
        }        

        private void OnStopped()
        {
            Log.CloseAndFlush();
        }        
    }

    public class JwtInHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtInHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string cookie = context.Request.Cookies["JwtBearer"];

            if (cookie != null) {
                if (!context.Request.Headers.ContainsKey("Authorization")) {
                    context.Request.Headers.Append("Authorization", "Bearer " + cookie);
                }
            }

            await _next.Invoke(context);
        }
    }
}