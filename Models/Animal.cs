using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Animal
    {
        [Key]
        public int id_animal { get; set; }
        public string nombre { get; set; }
        public string sexo { get; set; }   // 'M' o 'H'
        public int edadValor { get; set; }
        public string edadUnidad { get; set; }  // 'M' o 'A'
        public byte[]? imagen { get; set; } // se guarda como varbinary(MAX)
        public int id_especie { get; set; }
        public int id_tamano { get; set; }
        public int id_estadoAnimal { get; set; }
        public int? id_persona { get; set; }
        public int? id_pension { get; set; }
        public int id_usuario { get; set;}
        public DateTime fechaIngreso { get; set; } = DateTime.Now;
        public DateTime? fechaAdopcion { get; set; }
        public string? historia { get; set; }
        public string? seguimiento { get; set; }
         
    }
}
