using GitHub.CodeQL.Analytics.Data.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.CodeQL.Analytics.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Drawing.Printing;
using GitHub.CodeQL.Analytics.Cli.Constants;
using System.IO.Pipes;

namespace GitHub.CodeQL.Analytics.Cli.Commands.Reports
{
    internal class MarkdownReport : MarkdownElements
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<MarkdownReport> _log;
        private readonly AnalyticsContext _ctx;
        private IList<Analyses> _analyses;
        private string _generatedChartPath { get; set; }

        public MarkdownReport(IServiceProvider provider, AnalyticsContext ctx, IList<Analyses> analyses)
        {
            _provider = provider;
            _log = provider.GetRequiredService<ILogger<MarkdownReport>>();
            _ctx = ctx;
            _analyses = analyses;

        }

        public List<Rules> GenerateTopTenResults(Analyses analysis, StreamWriter writer)
        {
            _log.LogInformation("Generating top ten results");
            List<Rules> topten = analysis.Results.GroupBy(o => o.Rule).OrderByDescending(p => p.Count()).Select(k => k.Key).Take(10).ToList();

            writer.WriteLine("| Rule | Count | Description |");
            writer.WriteLine("|---|---|---|");


            foreach (var rule in topten)
            {
                writer.Write(" | ");
                writer.Write(rule.Id.ToString() + " | ");
                writer.Write(rule.Results.Count + " | ");
                writer.Write("**Rule Name:** " + rule.RuleName + "<br>");
                writer.Write("**Description:** " + rule.FullDescription + "<br>");
                writer.Write("**Severity:** " + rule.ProblemSeverity + "<br>");
                writer.Write("**Precision:** " + rule.RulePrecision + "<br>");
                writer.Write("**Tags:** ");
                foreach (Tags tag in rule.RuleTags)
                {
                    if (tag != rule.RuleTags.LastOrDefault())
                        writer.Write(tag.Name + " , ");
                    else
                        writer.Write(tag.Name);
                }

                writer.WriteLine(" | ");
            }
            writer.WriteLine();

            _log.LogInformation("Successfully generated top ten results");
            return topten;
        }

        public string GenerateTopTenPieChart(Analyses analysis, List<Rules> topten)
        {

            var plt = new ScottPlot.Plot(1200, 800);
            double totalResults = Convert.ToDouble(analysis.Results.Count());
            var values = new List<Double>();
            var labels = new List<string>();
            double toptentotal = 0;
            foreach (var rule in topten)
            {
                values.Add(rule.Results.Count);
                labels.Add(rule.Id);
                toptentotal += rule.Results.Count;

            }
            double other = totalResults - toptentotal;
            if (other < 0)
            {
                throw new ArithmeticException("Incorrect arithmetic when creating top ten pie chart");

            }
            if (other > 0)
            {
                values.Add(other);
                labels.Add("Other");
            }

            var pie = plt.AddPie(values.ToArray());
            pie.SliceLabels = labels.ToArray();
            pie.ShowPercentages = true;
            pie.ShowValues = false;
            pie.ShowLabels = false;
            plt.Legend();
            string imagename = analysis.AnalysisId + "toptenpiechart.png";
            return plt.SaveFig(Path.Combine(_generatedChartPath, imagename));

        }

        public void DiffTwoAnalyses(Analyses current, Analyses previous, StreamWriter writer)
        {
            string heading = $"Difference in results between query pack version {current.PackVersion} and {previous.PackVersion}";
            base.createHeading(heading, 3, writer);
            var currentResults = current.Results.GroupBy(o => o.Rule).Select(k => new { V = k.Key.Results.Count(), k.Key.Id }).ToList();
            var previousResults = previous.Results.GroupBy(o => o.Rule).Select(k => new { V = k.Key.Results.Count(), k.Key.Id }).ToList();
            writer.WriteLine($"Total number of alerts for current analysis using {current.QueryPack} with query pack version {current.PackVersion} : ***{current.Results.Count}*** ");
            writer.WriteLine($"Total number of alerts for previous analysis using {previous.QueryPack} with query pack version {previous.PackVersion}: ***{previous.Results.Count}*** ");

            List<Tuple<string, int>> analysed = new List<Tuple<string, int>>();
            foreach (var result in currentResults)
            {
                var prev = previousResults.Where(p => p.Id == result.Id)?.ToList()?.FirstOrDefault();

                int diff;
                if (prev == null)
                {
                    _log.LogInformation($"New occurence of alert for rule {result.Id} in analysis {current.AnalysisId} for pack version {current.PackVersion}.");
                    diff = result.V;
                }
                else
                {
                    diff = result.V - prev.V;
                }

                analysed.Add(new Tuple<string, int>(result.Id, diff));
                if (diff > 0 || diff < 0)
                {
                    if (diff > 0)
                    {
                        writer.WriteLine($"- Addition of {diff} alerts for Rule {result.Id}");
                    }
                    if (diff < 0)
                    {
                        writer.WriteLine($"- Removal of {diff} alerts for Rule {result.Id}");

                    }
                }
            }
            foreach (var result in previousResults)
            {
                var delta = analysed.Where(p => p.Item1 == result.Id)?.ToList();
                //If it's not in current delta, then it has been removed from current analysis 
                if (delta.Count == 0)
                {
                    writer.WriteLine($"- Removal of alert {result.Id} from current analysis. Previous alert number: {result.V}. Current alert number 0.");
                }
            }

        }
        public string GenerateSummaryChart(List<Tuple<string, int>> totalResult, string reportTitle)
        {
            var plt = new ScottPlot.Plot(600, 400);
            List<string> labels = new List<string>();
            List<double> values = new List<double>();
            List<double> position = new List<double>();

            for (var i = 0; i < totalResult.Count; i++)
            {
                labels.Add(totalResult[i].Item1.ToString());
                values.Add(Convert.ToDouble(totalResult[i].Item2));
                position.Add(Convert.ToDouble(i));
            }

            var bar = plt.AddBar(values.ToArray(), position.ToArray());

            bar.ShowValuesAboveBars = true;
            plt.XTicks(position.ToArray(), labels.ToArray());
            plt.SetAxisLimits(yMin: 0);

            string imagename = Path.GetRandomFileName() + "resultsummarybarchart.png";
            return plt.SaveFig(Path.Combine(_generatedChartPath, imagename));
        }

        public void GeneratePerfomanceSummary(Analyses analysis, StreamWriter writer)
        {
            var evalTimes = _ctx.EvaluationSet.Where(a => a.AnalysisId == analysis.AnalysisId).OrderByDescending(a => a.EvaluationTimeMilliseconds).Take(10).ToList();
            var evalResults = _ctx.EvaluationSet.Where(a => a.AnalysisId == analysis.AnalysisId).OrderByDescending(a => a.ResultSize).Take(10).ToList();

            _log.LogInformation("Generating performance summary");

            string perfheading = "Top ten longest running predicates";
            base.createHeading(perfheading, 3, writer);
            writer.WriteLine("| Predicate Name | Time in milliseconds | Result Size | Query Causing Work");
            writer.WriteLine("|---|---|---|---|");

            foreach (var time in evalTimes)
            {

                writer.Write(" | ");
                writer.Write(time.PredicateName + " | ");
                writer.Write(time.EvaluationTimeMilliseconds + " | ");
                writer.Write(time.ResultSize + " | ");
                writer.Write(Path.GetFileName(time.QueryCausingWork) + " | \n");
            }

            writer.WriteLine();
            _log.LogInformation("Successfully generated performance times summary");

            _log.LogInformation("Generating result size summary");

            string resultsizeheading = "Top ten largest predicate result sizes";
            base.createHeading(resultsizeheading, 3, writer);
            writer.WriteLine("| Predicate Name | Time in milliseconds | Result Size | Query Causing Work");
            writer.WriteLine("|---|---|---|---|");

            foreach (var time in evalResults)
            {
                writer.Write(" | ");
                writer.Write(time.PredicateName + " | ");
                writer.Write(time.EvaluationTimeMilliseconds + " | ");
                writer.Write(time.ResultSize + " | ");
                writer.Write(Path.GetFileName(time.QueryCausingWork) + " | \n");
            }

            writer.WriteLine();
            _log.LogInformation("Successfully result size summary");
        }

        public void GenerateFileSummary(Analyses analysis, StreamWriter writer)
        {
            var filesummary = analysis.Results.GroupBy(o => o.ArtefactFileLocation).Select(k => new { V = k.ToList().Count(), k.Key }).OrderByDescending(k => k.V).Take(10);

            string heading = "Files with most rule contraventions";
            base.createHeading(heading, 3, writer);

            writer.WriteLine("| File Name | Alert Count |");
            writer.WriteLine("|---|---|");
            foreach (var result in filesummary)
            {

                writer.Write(" | ");
                writer.Write(result.Key + " | ");
                writer.Write(result.V + " | \n ");

            }
        }

        public void GenerateTotalTimeSummary(List<Tuple<Guid, string, string, int>> totalTimes, StreamWriter writer)
        {
            writer.WriteLine();
            string heading = "**Total Analysis Time**";
            ;

            writer.WriteLine("| AnalysisId | Pack Version | Duration (ms) | Total no. of rules");
            writer.WriteLine("|---|---|---|---|");
            foreach (var result in totalTimes)
            {
                writer.Write(" | ");
                writer.Write(result.Item1 + " | ");
                writer.Write(result.Item2 + " | ");
                writer.Write(result.Item3 + " | ");
                writer.Write(result.Item4 + " | \n");
            }
            writer.WriteLine();
        }

        public void GenerateReport(string filepath, string reportTitle, string db, string querypack)
        {
            using (System.IO.FileStream fs = System.IO.File.Create(filepath))
            {
                using (var writer = new StreamWriter(fs, Encoding.UTF8))
                {
                    _generatedChartPath = base.createChartsFolder(filepath);
                    _log.LogInformation($"All charts generated in this report will be saved in {_generatedChartPath}");
                    base.createHeading(reportTitle, 1, writer);

                    //sort 
                    _analyses = _analyses.OrderBy(item => item.PackVersion).ThenBy(item => item.AnalysisDate).ToList();
                    List<Tuple<string, int>> totalResult = new List<Tuple<string, int>>();
                    List<Tuple<Guid, string, string, int>> totalTime = new List<Tuple<Guid, string, string, int>>();
                    string subheadingl3 = "Analysis of CodeQL database **" + db + "** using query pack **" + querypack + "**";
                    writer.WriteLine(subheadingl3);

                    string sectionheading = "Summary of results for each analysis";
                    base.createHeading(sectionheading, 2, writer);
                    foreach (var analysis in _analyses)
                    {

                        totalResult.Add(new Tuple<string, int>(analysis.PackVersion, analysis.Results.Count()));
                        totalTime.Add(new Tuple<Guid, string, string, int>(analysis.AnalysisId, analysis.PackVersion, analysis.TotalAnalysisTime, analysis.Rules.Count));

                    }
                    string generatedSummaryChartFile = GenerateSummaryChart(totalResult, reportTitle);
                    GenerateTotalTimeSummary(totalTime, writer);

                    writer.WriteLine("**Results Summary Chart**");
                    writer.WriteLine(" <br> ");
                    writer.WriteLine("![Results Summary Chart](./charts/" + Path.GetFileName(generatedSummaryChartFile) + ")");//to do - make this less hard coded

                    foreach (var analysis in _analyses)
                    {
                        string subheading = "Results for analysis";
                        if (analysis.PackVersion != null)
                            subheading += " using Coding Standards Pack Version " + analysis.PackVersion;
                        base.createHeading(subheading, 2, writer);



                        string analysisid = "Analysis Id: " + analysis.AnalysisId + "<br>";
                        string language = "Language Analysed: " + analysis.LanguageAnalysed + "<br>";
                        writer.WriteLine(language);

                        string analysisDate = "Analysis Date: " + analysis.AnalysisDate + "<br>";
                        writer.WriteLine(analysisDate);

                        if (analysis.CodeQLVersion != null)
                        {
                            string codeqlVersion = "This analysis was done using CodeQL CLI Version " + analysis.CodeQLVersion + "<br>";
                            writer.WriteLine(codeqlVersion);
                        }

                        string totalCount = "The total number of alerts for this analysis was " + analysis.Results.Count() + "<br>";


                        writer.WriteLine(totalCount);
                        if (analysis.Results.Count > 0)
                        {
                            string toptenheading = "Top Ten Results";
                            base.createHeading(toptenheading, 3, writer);
                            List<Rules> toptenrules = GenerateTopTenResults(analysis, writer);
                            string toptenpiechart = GenerateTopTenPieChart(analysis, toptenrules);
                            writer.WriteLine("**Top Ten Pie Chart**");
                            writer.WriteLine("![Top Ten Pie Chart](./charts/" + Path.GetFileName(toptenpiechart) + ")");//to do - make this less hard coded
                            GenerateFileSummary(analysis, writer);
                            //compare with previous if applicable 
                            if (_analyses.Count > 1 && _analyses.IndexOf(analysis) > 0)
                            {
                                int prevIndex = _analyses.IndexOf(analysis) - 1;
                                Analyses prev = _analyses[prevIndex];
                                DiffTwoAnalyses(analysis, _analyses[prevIndex], writer);
                            }
                        }
                        else
                        {
                            writer.WriteLine("**This analysis returned zero alerts.**");
                        }
                        GeneratePerfomanceSummary(analysis, writer);

                    }
                }
            }
        }
    }
}

