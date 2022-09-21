using Castle.Core.Internal;
using Castle.Core.Logging;
using GitHub.CodeQL.Analytics.Cli.Commands.Database.Load;
using GitHub.CodeQL.Analytics.Data.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace GitHub.CodeQL.Analytics.Cli.Services
{
    public  class SarifService
    {
        private readonly ILogger<SarifService> _log;
        public SarifService(ILogger<SarifService> log)
        {
            _log = log;
        }

        public Tuple<IList<Rules>, IList<Tags>> RulesAndTagsMapper(JsonArray rulesObject, string packversion, Guid AnalysisId)
        {
            List<Rules> rules = new List<Rules>();
            List<Tags> tags = new List<Tags>();
            foreach (JsonObject obj in rulesObject)
            {
                Rules rule = new Rules()
                {
                    RuleId = Guid.NewGuid(),
                    Id = obj["id"].ToString(),
                    FullDescription = obj["fullDescription"]["text"].ToString(),
                    ShortDescription = obj["shortDescription"]["text"].ToString(),
                    Kind = obj["properties"]["kind"].ToString(),
                    RulePrecision = obj["properties"]["precision"].ToString(),
                    ProblemSeverity = obj["properties"]["problem.severity"].ToString(),
                    RuleName = obj["name"].ToString(),
                    PackVersion = packversion,
                    AnalysisId = AnalysisId
                };
                rules.Add(rule);

                //  _db.RuleSet.Add(rule);
                //   await _db.SaveChangesAsync();
                //  _log.LogInformation("Added Rule with Id {RuleId}", rule.RuleId);

                JsonArray tagsArray = obj["properties"]["tags"].AsArray();
                if (!tagsArray.IsNullOrEmpty())
                {
                    foreach (var result in tagsArray)
                    {
                        Tags tag = new Tags()
                        {
                            TagId = Guid.NewGuid(),
                            Name = result.ToString(),
                            RuleId = rule.RuleId

                        };
                        tags.Add(tag);
                        //_db.TagSet.Add(tag);
                        //await _db.SaveChangesAsync();
                        //_log.LogInformation("Added Tag with Id {TagId}", tag.TagId);
                    }
                }
            }
            return new Tuple<IList<Rules>, IList<Tags>>(rules, tags);

        }


        public IList<Results> ResultsMapper(JsonArray resultsObject, IList<Rules> rules, Guid AnalysisId)
        {
            List<Results> results = new List<Results>();
            foreach (JsonObject obj in resultsObject)
            {
                //validation 
                if (obj["locations"].AsArray().Count > 1)
                {
                    throw new Exception("Invalid result object: found multiple locations associated to thos result.");
                }

                Results result = new Results()
                {
                    Id = obj["ruleId"].ToString(),
                    AlertMessage = obj["message"]["text"].ToString(),
                    ArtefactFileLocation = obj["locations"][0]["physicalLocation"]["artifactLocation"]["uri"].ToString(),

                    ResultId = Guid.NewGuid(),
                    AnalysisId = AnalysisId
                };

                var getRuleForResult = rules.Where(p => p.Id == result.Id);
                if (getRuleForResult.Count() > 1)
                    throw new Exception("Invalid result object: found multiple rules associated to this result");
                result.RuleId = getRuleForResult.First().RuleId;
                results.Add(result);
            }
            return results;
        }
    }
}

