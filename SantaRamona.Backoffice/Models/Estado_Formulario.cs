using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Backoffice.Models
{
    public class Estado_Formulario
    {
        public int id_estadoFormulario { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string descripcion { get; set; }
    }
}
