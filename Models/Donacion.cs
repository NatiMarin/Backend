using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Donacion
    {
        [Key]
        public int id_donacion { get; set; }

        // 'M' = Medicamentos | 'I' = Insumos
        [Required]
        [RegularExpression("^(M|I)$", ErrorMessage = "El tipo debe ser 'M' (Medicamentos) o 'I' (Insumos).")]
        public string tipo { get; set; }

        [StringLength(40)]
        public string? descripcion { get; set; }
    }
}
