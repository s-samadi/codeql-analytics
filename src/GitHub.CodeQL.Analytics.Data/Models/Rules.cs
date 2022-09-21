using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GitHub.CodeQL.Analytics.Data.Models
{
    [Table("Rules", Schema = "QL")]
    public class Rules
    {
        [Key]
        [Column("RuleId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
        public Guid RuleId { get; set; }

        [Column("Id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [Column("RuleName")]
        [JsonPropertyName("name")]
        public string RuleName { get; set; }

        [Column("ShortDescription")]
        [JsonPropertyName("shortDescription")]
        public string ShortDescription { get; set; }

        [Column("FullDescription")]
        [JsonPropertyName("fullDescription")]
        public string FullDescription { get; set; }

        [Column("Kind")]
        [JsonPropertyName("kind")]
        public string Kind { get; set; }

        [Column("RulePrecision")]
        [JsonPropertyName("precision")]
        public string RulePrecision { get; set; }

        [Column("ProblemSeverity")]
        [JsonPropertyName("problem.severity")]
        public string ProblemSeverity { get; set; }

        [Column("PackVersion")]
        public string PackVersion { get; set; }

        [Column("AnalysisId")]
        public Guid AnalysisId { get; set; }

        public virtual Analyses Analysis { get; set; }

        public virtual IList<Tags> RuleTags { get; set; }
        public virtual  IList<Results> Results { get; set; }

    }
}
