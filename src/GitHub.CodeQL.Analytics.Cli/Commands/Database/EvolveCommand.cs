using System;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using GitHub.CodeQL.Analytics.Data.Services;
using Microsoft.Data.SqlClient;
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

    public ValueTask ExecuteAsync(IConsole console) {
        var dbType = _config.GetValue<string>("DatabaseType");
        var cnxStr = _config.GetConnectionString($"Analytics.{dbType}");
        DbConnection cnx = dbType switch {
            "Sqlite" => new SqliteConnection(cnxStr),
            "SqlServer" => new SqlConnection(cnxStr),
            _ => throw new ApplicationException($"database type '{dbType}' is not supported")
        };
        var evolve = new Evolve.Evolve(cnx) {
            EmbeddedResourceAssemblies = new [] {
                Assembly.GetAssembly(typeof(AnalyticsContext))
            },
            EmbeddedResourceFilters = new [] {
                $"GitHub.CodeQL.Analytics.Data.Migrations.{dbType}"
            },
            IsEraseDisabled = true
        };
        evolve.Migrate();

        return ValueTask.CompletedTask;
    }
}