using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Raza // sacar para no tener que agregar ++
    {
        [Key]
        public int id_raza { get; set; }
        public string raza { get; set; }
    }
}
