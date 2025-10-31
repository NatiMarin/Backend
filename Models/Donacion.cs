using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Donacion
    {
        [Key]
        public int id_donacion { get; set; }

        // 'M' = Medicamentos | 'I' = Insumos | B = BANCO | P = No Bancario
        [Required]
        [RegularExpression("^(M|I|B|P)$", ErrorMessage = "Seleccione 'M' (Medicamento) o 'I' (Insumo) o 'B' (Banco) o 'P' (No Bancario).")] 
        public string tipo { get; set; }

        [StringLength(40)]
        public string? descripcion { get; set; }
    }
}
