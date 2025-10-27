using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SantaRamona.Models
{
    [Table("USUARIO")]
    public class Usuario
    {
        [Key] public int id_usuario { get; set; }
        public string clave { get; set; } = null!;
        public string email { get; set; } = null!;
        public string nombre { get; set; } = null!;
        public string apellido { get; set; } = null!;
        public string? direccion { get; set; }
        public int? altura { get; set; }
        public string? departamento { get; set; }
        public int telefono { get; set; }
        public DateTime fechaAlta { get; set; }
        public int id_estadoUsuario { get; set; }
    }
}
