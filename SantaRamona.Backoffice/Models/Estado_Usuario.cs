using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Backoffice.Models
{
    public class Estado_Usuario
    {
        [Key]
        public int id_estadoUsuario { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string descripcion { get; set; }
        public bool enUso { get; set; }  // Indica si el estado está asociado a algún usuario
    }
}
