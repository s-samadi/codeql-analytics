using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GitHub.CodeQL.Analytics.Data.Models; 

[Table("person")]
public class Person {
    
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    [Column("name")]
    public string Name { get; set; }

    public virtual List<Animal> PetList { get; } = new();

}