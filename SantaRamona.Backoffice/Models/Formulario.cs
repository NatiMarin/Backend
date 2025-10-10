namespace SantaRamona.Backoffice.Models
{
    public class Formulario
    {
        public int id_formulario { get; set; }
        public int id_persona { get; set; }
        public int id_tipoFormulario { get; set; }
        public DateTime fechaAltaFormulario { get; set; }
        public int? id_usuario { get; set; }
        public int id_estadoFormulario { get; set; }
    }
}
