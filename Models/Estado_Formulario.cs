using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Estado_Formulario
    {
        [Key]
        public int id_estadoFormulario { get; set; }
        public string descripcion { get; set; }
    }
}
