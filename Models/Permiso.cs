using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Permiso
    {
        [Key]
        public int id_permiso { get; set; }
        public string descripcion { get; set; }
    }
}
