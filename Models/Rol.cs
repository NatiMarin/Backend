using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SantaRamona.Models
{
    [Table("ROL")]
    public class Rol
    {
        [Key] public int id_rol { get; set; }
        public string descripcion { get; set; } = null!;
    }
}
