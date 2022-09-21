using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.CodeQL.Analytics.Data.Models
{
    [Table("EvaluationTime", Schema = "QL")]
    public class EvalutationTime
    {
        [Key]
        [Column("EvaluationId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid EvaluationTimeId { get; set; }

        [Column("PredicateName")]
        public string PredicateName { get; set; }

        [Column("QueryCausingWork")]
        public string QueryCausingWork { get; set; }

        [Column("EvaluationStrategy")]
        public string EvaluationStrategy { get; set; }

        [Column("EvaluationTimeMilliseconds")]
        public double? EvaluationTimeMilliseconds { get; set; }

        [Column("AnalysisId")]
        public Guid AnalysisId { get; set; }

        [Column("ResultSize")]
        public double? ResultSize { get; set; }
    }
}
