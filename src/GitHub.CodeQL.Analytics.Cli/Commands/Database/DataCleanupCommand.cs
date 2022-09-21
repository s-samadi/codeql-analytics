using Castle.Core.Internal;
using GitHub.CodeQL.Analytics.Cli.Commands.Database.Load;
using GitHub.CodeQL.Analytics.Cli.Constants;
using GitHub.CodeQL.Analytics.Cli.Services;
using GitHub.CodeQL.Analytics.Data.Models;
using GitHub.CodeQL.Analytics.Data.Services;
using Microsoft.Extensions.Logging;
using ScottPlot.Plottable;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace GitHub.CodeQL.Analytics.Cli.Commands.Database
{
    [Command("db data cleanup", Description = "Copies and assigns Analysis to sarif and log files")]
    public class DataCleanupCommand : ICommand
    {
        private readonly ILogger<SarifData> _log;
        private readonly SarifService _sarifService;
        private readonly AnalyticsContext _db;
        public DataCleanupCommand(ILogger<SarifData> log, AnalyticsContext db)
        {
            _db = db;
            _log = log;
        }
        public Guid AnalysisId { get; } = Guid.NewGuid();

        [CommandOption("analysis-type", Description = "sarif-only or sarif-and-logs", IsRequired = true)]
        public string AnalysisType { get; set; }

        [CommandOption("sarif", Description = "sarif file path", IsRequired = true)]
        public string SarifFile { get; set; }

        [CommandOption("language", Description = "language analysed", IsRequired = true)]
        public string LanguageAnalysed { get; set; }

        [CommandOption("querypack", Description = "query pack used for analysis", IsRequired = true)]
        public string QueryPack { get; set; }

        [CommandOption("codeqldatabase", Description = "name of CodeQL database", IsRequired = true)]
        public string CodeQLDatabase { get; set; }

        [CommandOption("evaluator", Description = "evaluator.json file path", IsRequired = false)]
        public string EvaluatorFile { get; set; }

        [CommandOption("evaluator-summary", Description = "evaluator-summary.json file path", IsRequired = false)]
        public string EvaluatorSummaryFile { get; set; }

        [CommandOption("pack-version", Description = "CodeQL Pack Version", IsRequired = false)]
        public string PackVersion { get; set; }

        [CommandOption("analysis-date", Description = "Date of analysis", IsRequired = false)]
        public DateTime? AnalysisDate { get; set; }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            //Validate input
            ValidateInput();

            //Generate UserAnalysisId 
       

            Analyses analysis = new Analyses()
            {
                AnalysisId = AnalysisId,
                AnalysisType = AnalysisType.ToLower(),
                PackVersion = PackVersion,
                LanguageAnalysed = LanguageAnalysed,
                QueryPack = QueryPack,
                AnalysisDate = AnalysisDate,
                CodeQLDatabaseName = CodeQLDatabase
            };

            //Create and copy file to temp location
            var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"EvaluatorLogs-{AnalysisId}"));
            var evaluatorfile = Path.Combine(dir.FullName, Path.GetFileName(EvaluatorFile));               
            File.Copy(EvaluatorFile, evaluatorfile);
            var evaluatorsummaryfile = Path.Combine(dir.FullName, Path.GetFileName(EvaluatorSummaryFile));
            File.Copy( EvaluatorSummaryFile, evaluatorsummaryfile);
            var sariffile = Path.Combine(dir.FullName, Path.GetFileName(SarifFile));
            File.Copy(SarifFile, sariffile);
            //Create Analysis record in db 
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.AnalysesSet.Add(analysis);
                    await _db.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    if (File.Exists(evaluatorfile))
                        File.Delete(evaluatorfile);
                    if (File.Exists(evaluatorsummaryfile))
                        File.Delete(evaluatorsummaryfile);
                    throw new Exception("Error inserting data in SQL Database - Rolling back transaction", ex);
                }
            }
            Console.WriteLine($"Analysis Id is {AnalysisId}");
            Console.WriteLine($"Evaluator file location is: {evaluatorfile}");
            Console.WriteLine($"Evaluator summary file location is: {evaluatorsummaryfile}");
            Console.WriteLine($"Sarif file location is: {sariffile}");

        }

        public void ValidateInput()
        {
            //Sanitize and validate input
            if ((!EvaluatorFile.IsNullOrEmpty() && EvaluatorSummaryFile.IsNullOrEmpty()) || (EvaluatorFile.IsNullOrEmpty() && !EvaluatorSummaryFile.IsNullOrEmpty()))
                throw new ArgumentException("Please provide both evaluator json file and evaluator-summary json file.");
            if (AnalysisType == AnalysisTypeValues.SarifOnly && (!EvaluatorFile.IsNullOrEmpty() || !EvaluatorSummaryFile.IsNullOrEmpty()))
                throw new ArgumentException("Log files provided. Please choose the sarif-and-log option if you would like to analyse the log files.");
            if (AnalysisType == AnalysisTypeValues.SarifOnly && AnalysisDate == null)
                throw new ArgumentException("Please provide CodeQL analysis date.");
            if (AnalysisType != AnalysisTypeValues.SarifOnly && AnalysisType != AnalysisTypeValues.SarifAndLogs)
                throw new ArgumentException("Please provide a valid Analysis Type.");
            if (AnalysisType == AnalysisTypeValues.SarifAndLogs && (EvaluatorFile.IsNullOrEmpty() || EvaluatorSummaryFile.IsNullOrEmpty()))
                throw new ArgumentException("Please provide both evaluator json file and evaluator-summary json file for analysis type sarif-and-logs.");
        }
   
    }
}
