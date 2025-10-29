using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Donacion
    {
        [Key]
        public int id_donacion { get; set; }

        // 'M' = Medicamentos | 'I' = Insumos | B = BANCO | MP = Mercado Pago
        [Required]
        [RegularExpression("^(M|I|B|MP)$", ErrorMessage = "Seleccione 'M' (Medicamento) o 'I' (Insumo) o 'B' (Banco) o 'MP' (Mercado Pago).")] 
        public string tipo { get; set; }

        [StringLength(40)]
        public string? descripcion { get; set; }
    }
}
