using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.CodeQL.Analytics.Data.Models
{
    [Table("Analyses", Schema ="QL")]
    public class Analyses
    {
        [Key]
        [Column("AnalysisId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid AnalysisId { get; set; }

        [Column("TotalAnalysisTime")]
        public string TotalAnalysisTime { get; set; }

        [Column("PackVersion")]
        public string PackVersion { get; set; }

        [Column("AnalysisDate")]
        public DateTime? AnalysisDate { get; set; }

        [Column("CodeQLVersion")]
        public string CodeQLVersion { get; set; }

        [Column("AnalysisType")]
        public string AnalysisType { get; set; }

        [Column("LanguageAnalysed")]
        public string LanguageAnalysed { get; set; }

        [Column("CodeQLDatabaseName")]
        public string CodeQLDatabaseName { get; set; }

        [Column("QueryPack")]
        public string QueryPack { get; set; }
        
        public virtual IList<Results> Results { get; set; }
        public virtual IList<Rules> Rules { get; set; }
    }
}
