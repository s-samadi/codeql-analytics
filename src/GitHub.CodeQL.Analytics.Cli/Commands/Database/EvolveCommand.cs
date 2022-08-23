using System.Reflection;
using System.Threading.Tasks;
using GitHub.CodeQL.Analytics.Data.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace GitHub.CodeQL.Analytics.Cli.Commands.Database;

[Command("db evolve", Description = "Apply database upgrade scripts")]
public class EvolveCommand : ICommand {

    private readonly IConfiguration _config;

    public EvolveCommand(IConfiguration config) {
        _config = config;
    }

    [CommandOption("name", Description = "Name of database to upgrade", IsRequired = true)]
    public string DatabaseName { get; set; }

    public ValueTask ExecuteAsync(IConsole console) {
        var cnx = new SqliteConnection(_config.GetConnectionString(DatabaseName));
        var evolve = new Evolve.Evolve(cnx) {
            EmbeddedResourceAssemblies = new [] {
                Assembly.GetAssembly(typeof(AnalyticsContext))
            },
            EmbeddedResourceFilters = new [] {
                "GitHub.CodeQL.Analytics.Data.Migrations.Sqlite"
            },
            IsEraseDisabled = true
        };
        evolve.Migrate();

        return ValueTask.CompletedTask;
    }
}