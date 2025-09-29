using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Usuario
    {
        [Key]
        public int id_usuario { get; set; }
        public string clave { get; set; }
        public string email { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string direccion { get; set; }
        public string altura { get; set; }
        public string departamento { get; set; }
        public int telefono1 { get; set; }
        public int telefono2 { get; set; }
        public DateTime fechaAlta { get; set; }
        public int id_estadoUsuario { get; set; }

    }
}
