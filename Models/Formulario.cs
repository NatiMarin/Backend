using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Formulario
    {
        [Key]
        public int id_formulario { get; set; }
        public int id_persona { get; set; }
        public int id_tipoFormulario { get; set; }
        public DateTime fechaAltaFormulario { get; set; }
        public int? id_usuario { get; set; }
        public int id_estadoFormulario { get; set; }

    }
}
