using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Backoffice.Models
{
    public class Animal
    {
        public int id_animal { get; set; }
        public string nombre { get; set; }

        [Required(ErrorMessage = "El Nombre es obligatoria")]
        public int edad { get; set; }
        public int id_especie { get; set; }
        public int id_tamano { get; set; }
       
        public int id_raza { get; set; }
        public int id_estado { get; set; }
        public int? id_persona { get; set; }
        public int id_usuario { get; set; }
        public string historia { get; set; }
        public int? id_pension { get; set; }
    }
}