using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Estado_Animal
    {
        [Key]
        public int id_estadoAnimal { get; set; }
        public string estado { get; set; }
    }
}
