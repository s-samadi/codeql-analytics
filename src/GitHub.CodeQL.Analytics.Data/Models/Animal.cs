using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GitHub.CodeQL.Analytics.Data.Models; 

[Table("animal")]
public abstract class Animal {
    
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    [Column("name")]
    public string Name { get; set; }

    [Column("person_id")]
    public long PersonId { get; set; }
    public virtual Person Person { get; set; }

    public abstract string Speak();

}