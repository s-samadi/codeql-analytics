using System.Threading.Tasks;
using GitHub.CodeQL.Analytics.Cli.Services;
using GitHub.CodeQL.Analytics.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Filters;
using Typin;

namespace GitHub.CodeQL.Analytics.Cli;

internal class Program {

    #region Entry Point

    private static async Task Main() {
        await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .ConfigureLogging(options => {
                var logger = new LoggerConfiguration()
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                    .Enrich.FromLogContext()
                    .Filter.ByExcluding(Matching.FromSource("Microsoft.EntityFrameworkCore"))
                    .Filter.ByExcluding(Matching.FromSource("Typin"))
                    .CreateLogger();
                options.AddSerilog(logger);
            })
            .ConfigureServices(services => {
                var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
                var cfg = builder.Build();
                services.Add(new ServiceDescriptor(typeof(IConfiguration), cfg));
                services.AddSingleton<SecretService>();

                services.AddDbContext<AnalyticsContext>(
                    options => {
                        var cnxString = cfg.GetConnectionString("analytics");
                        options.UseSqlite(cnxString);

                        options.UseLazyLoadingProxies();
                    }
                );
            })
            .Build()
            .RunAsync();
    }

    #endregion

}