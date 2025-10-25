using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Models
{
    public class Tipo_Formulario
    {
        [Key]
        public int id_tipoFormulario { get; set; }

        [Required(ErrorMessage = "El tipo de formulario es obligatorio.")]
        [StringLength(100, ErrorMessage = "El tipo no puede superar los 50 caracteres.")]
        public string tipo { get; set; } = string.Empty;

        // Estado: solo 'Activo' o 'Inactivo', controlado desde el front con botones
        [StringLength(50)]
        [RegularExpression("^(Activo|Inactivo)$", ErrorMessage = "El estado debe ser 'Activo' o 'Inactivo'.")]
        public string? Estado { get; set; } = "Activo";
    }
}
