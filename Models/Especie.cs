using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Especie
    {
        [Key]
        public int id_especie { get; set; }
        public string especie { get; set; }
    }
}
