using System.ComponentModel.DataAnnotations.Schema;

namespace SantaRamona.Models
{
    public class Usuario_Rol
    {
        public int id_usuario { get; set; }
        public int id_rol { get; set; }

        // Relaciones
        public Usuario? Usuario { get; set; }
        public Rol? Rol { get; set; }
    }
}
