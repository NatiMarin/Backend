using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Tipo_Formulario
    {
        [Key]
        public int id_tipoFormulario {  get; set; }
        public string descripcion { get; set; }
    }
}
