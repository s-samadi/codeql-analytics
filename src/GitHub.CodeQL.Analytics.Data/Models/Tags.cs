using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GitHub.CodeQL.Analytics.Data.Models
{
    [Table("Tags", Schema = "QL")]
    public class Tags
    {
        [Key]
        [Column("TagId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TagId { get; set; }

        [Column("RuleId")]
        public Guid RuleId { get; set; }

        [Column("TagName")]
        public string Name { get; set; }

        public virtual Rules Rule { get; set; }
    }
}
