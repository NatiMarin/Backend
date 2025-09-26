using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Estado_Usuario
    {
        [Key]
        public int id_estadoUsuario { get; set; }
        public string descripcion { get; set; }
    }
}
