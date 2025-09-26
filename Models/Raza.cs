using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Raza
    {
        [Key]
        public int id_raza { get; set; }
        public string raza { get; set; }
    }
}
