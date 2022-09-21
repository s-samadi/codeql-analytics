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
    [Table("Results", Schema = "QL")]
    public class Results
    {
        [Key]
        [Column("ResultId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ResultId { get; set; }

        [Column("RuleId")]
        public Guid RuleId { get; set; }

        [Column("Id")]
        public string Id { get; set; }

        [Column("AlertMessage")]
        public string AlertMessage { get; set; }

        [Column("ArtefactFileLocation")]
        public string ArtefactFileLocation { get; set; }

        [Column("AnalysisId")]
        public Guid AnalysisId { get; set; }

        public virtual Rules Rule { get; set; }
        public virtual Analyses Analysis { get; set; }

    }
}
