using System;
using System.Data;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using GitHub.CodeQL.Analytics.Cli.Services;
using GitHub.CodeQL.Analytics.Data.Models;
using GitHub.CodeQL.Analytics.Data.Services;
using GitHub.CodeQL.Analytics.Cli.Constants;
using Microsoft.Extensions.Logging;
using Typin;
using Typin.Attributes;
using Typin.Console;
using Castle.Core.Internal;
using System.Collections.Generic;
using System.Linq;


namespace GitHub.CodeQL.Analytics.Cli.Commands.Database.Load
{
    [Command("db load sarif-data")]
    public class SarifData : ICommand
    {
        private readonly ILogger<SarifData> _log;
        private readonly SarifService _sarifService;
        private readonly AnalyticsContext _db;

        [CommandOption("analysisid", Description = "analysis id generated from clean up step.", IsRequired = true)]
        public Guid AnalysisId { get; set; }

        [CommandOption("sarif", Description = "sarif file path", IsRequired = true)]
        public string SarifFile { get; set; }

        [CommandOption("evaluator", Description = "Cleaned up evaluator.json file path", IsRequired = false)]
        public string EvaluatorFile { get; set; }

        [CommandOption("evaluator-summary", Description = "Cleaned up evaluator-summary.json file path", IsRequired = false)]
        public string EvaluatorSummaryFile { get; set; }


        public SarifData(
            ILogger<SarifData> log,
            AnalyticsContext db,
            SarifService sarifService)
        {
            _db = db;
            _log = log;
            _sarifService = sarifService;
        }
        public async ValueTask ExecuteAsync(IConsole console)
        {
            Analyses analysis = _db.AnalysesSet.Where(a => a.AnalysisId == AnalysisId).ToList().FirstOrDefault();
            if(analysis == null)
            {
                throw new DataException($"Analysis does not exist for provided Analysis ID: {AnalysisId}");
            }

            //Process Sarif
            var sarifFile = SarifFile;

            var options = new JsonNodeOptions
            {
                PropertyNameCaseInsensitive = true
            };

            string jsonFile = File.ReadAllText(sarifFile); //convert sarif to string 
            JsonNode sarif = JsonNode.Parse(jsonFile, options);

            JsonArray rulesObject = sarif!["runs"][0]!["tool"]!["driver"]!["rules"]!.AsArray();
            Tuple<IList<Rules>, IList<Tags>> rulesAndTags = _sarifService.RulesAndTagsMapper(rulesObject, analysis.PackVersion, AnalysisId); ;
            IList<Rules> rules = rulesAndTags.Item1;
            IList<Tags> tags = rulesAndTags.Item2;

            JsonArray resultsObject = sarif!["runs"]![0]!["results"]!.AsArray();
            IList<Results> results = _sarifService.ResultsMapper(resultsObject, rules, analysis.AnalysisId);


            string totalTime = null;
            List<EvalutationTime> evaluations = new List<EvalutationTime>();
            if (analysis.AnalysisType == AnalysisTypeValues.SarifAndLogs)
            { 
                //Process Logs 
                try
                {
                    if (!File.Exists(EvaluatorFile))
                        throw new FileNotFoundException($"File {EvaluatorFile} not found.");
                    if (!File.Exists(EvaluatorSummaryFile))
                        throw new FileNotFoundException($"File {EvaluatorSummaryFile} not found.");

                    IEnumerable<string> lines = File.ReadAllLines(EvaluatorFile);

                    DateTime? startTime = null;
                    DateTime? endTime = null;
                    foreach (var line in lines)
                    {
                        JsonNode node = JsonNode.Parse(line, options);
                        if (node!["type"].ToString() == "LOG_HEADER")
                        {
                            analysis.AnalysisDate = (DateTime)node!["time"];
                            startTime = (DateTime)node!["time"];
                        }
                        if (node!["type"].ToString() == "LOG_FOOTER")
                            endTime = (DateTime)node!["time"];
                    }
                    if (startTime.HasValue && endTime.HasValue)
                    {
                        TimeSpan span = endTime.Value.Subtract(startTime.Value);

                        totalTime = span.TotalMilliseconds.ToString();
                        if (totalTime.IsNullOrEmpty())
                            throw new DataException("Error calculating total time.");
                    }
                    else
                    {
                        throw new DataException("Error reading evaluator log file.");
                    }
                    
                    //Process evaluator summary 
                    IEnumerable<string> fileLines = File.ReadAllLines(EvaluatorSummaryFile);
                    foreach (var line in fileLines)
                    {
                        JsonNode node = JsonNode.Parse(line, options);

                        if (!(node!["predicateName"] == null))
                        {
                            EvalutationTime evaluation = new EvalutationTime
                            {

                                AnalysisId = analysis.AnalysisId,
                                PredicateName = node!["predicateName"].ToString(),
                                QueryCausingWork = node!["queryCausingWork"].ToString(),
                                EvaluationTimeMilliseconds = (float?)node["millis"],
                                ResultSize = (long?)node["resultSize"],
                                EvaluationStrategy = node!["evaluationStrategy"].ToString(),
                                EvaluationTimeId = Guid.NewGuid()
                            };
                            evaluations.Add(evaluation);
                        }
                    }
                } catch(Exception ex)
                {
                    _log.LogError(ex.Message);
                    throw new Exception("Something went wrong processing evaluator logs: ", ex);
                }

                //Update analysis model
                analysis.CodeQLVersion = sarif!["runs"][0]!["tool"]!["driver"]!["semanticVersion"]!.ToString();
                if (analysis.PackVersion == null)
                    analysis.PackVersion = analysis.QueryPack;
                if(analysis.AnalysisType == AnalysisTypeValues.SarifAndLogs)
                {
                    analysis.TotalAnalysisTime = totalTime.ToString();
                }else
                {
                    analysis.TotalAnalysisTime = "N/A";
                }
                
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    //Save analysis
                    _db.AnalysesSet.Update(analysis);

                    foreach(Rules rule in rules)
                    {
                        _db.RuleSet.Add(rule);
                    }

                    foreach(Results res in results)
                    {
                        _db.ResultSet.Add(res);
                    }
                    foreach(Tags tag in tags)
                    {
                        _db.TagSet.Add(tag);
                    }
                    foreach(EvalutationTime eval in evaluations)
                    {
                        _db.EvaluationSet.Add(eval);
                    }
                    _db.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    throw new Exception("Error inserting data in SQL Database - Rolling back transaction", ex);
                }
            }

        }
    }
}
