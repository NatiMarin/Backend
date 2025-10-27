using System.ComponentModel.DataAnnotations.Schema;

namespace SantaRamona.Models
{
    [Table("USUARIO_ROL")]
    public class Usuario_Rol
    {
        public int id_usuario { get; set; }
        public int id_rol { get; set; }

        public Usuario? Usuario { get; set; }
        public Rol? Rol { get; set; }
    }
}
