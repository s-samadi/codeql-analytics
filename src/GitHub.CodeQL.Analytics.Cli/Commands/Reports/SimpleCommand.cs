using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitHub.CodeQL.Analytics.Data.Services;
using Microsoft.Extensions.Logging;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace GitHub.CodeQL.Analytics.Cli.Commands.Reports; 

[Command("report simple")]
public class SimpleCommand : ICommand {

    private readonly IServiceProvider _provider;
    private readonly ILogger<SimpleCommand> _log;
    private readonly AnalyticsContext _ctx;

    public SimpleCommand(IServiceProvider provider, ILogger<SimpleCommand> log, AnalyticsContext ctx) {
        _provider = provider;
        _log = log;
        _ctx = ctx;
    }
    [CommandOption("file", Description = "report file path", IsRequired = true)]
    public string FilePath { get; set; }

    [CommandOption("title", Description = "report title", IsRequired = true)]
    public string ReportTitle { get; set; }

    [CommandOption("querypack", Description = "Query pack to generate report for", IsRequired = true)]
    public string QueryPack { get; set; }

    [CommandOption("database", Description = "CodeQL database to generate report for", IsRequired = true)]
    public string CodeQLDatabase { get; set; }

    public async ValueTask ExecuteAsync(IConsole console) {
        await GetResults();
    }

    private async Task GetResults()
    {
        using (var ctx = _ctx) {
            var results = ctx.AnalysesSet.Where( p=> p.QueryPack == QueryPack && p.CodeQLDatabaseName == CodeQLDatabase ).ToList();
            string filepath = FilePath;

            if (!System.IO.File.Exists(filepath))
            {
                if (Path.GetExtension(filepath) != ".md")
                {
                    throw new FormatException("This file extension is not supported");
                }
                string reportTitle = ReportTitle;
   
                MarkdownReport report = new MarkdownReport(_provider, ctx, results); 
                report.GenerateReport(filepath, reportTitle, CodeQLDatabase, QueryPack);
            }
            else
            {
                Console.WriteLine("File \"{0}\" already exists.", filepath);
                return;
            }
        }
    }
}
