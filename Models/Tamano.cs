using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Tamano
    {
        [Key]
        public int id_tamano { get; set; }
        public string tamano { get; set; }
    }
}
