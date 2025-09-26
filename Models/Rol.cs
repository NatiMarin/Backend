using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Rol
    {
        [Key]
        public int id_rol { get; set; }
        public string descripcion { get; set; }
    }
}
