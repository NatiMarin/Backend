using System;
using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Backoffice.Models
{
    public class Pension
    {
        public int id_pension { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string nombre { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Ingrese un email válido.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string? email { get; set; }

        [StringLength(60, ErrorMessage = "Máximo 60 caracteres.")]
        public string? calle { get; set; }

        // En la API es string; lo dejamos string para permitir valores como "123A", "s/n", etc.
        [StringLength(10, ErrorMessage = "Máximo 10 caracteres.")]
        public string? altura { get; set; }

        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        public string? pisoCasa { get; set; }

        [StringLength(10, ErrorMessage = "Máximo 10 caracteres.")]
        public string? departamento { get; set; }

        [StringLength(60, ErrorMessage = "Máximo 60 caracteres.")]
        public string? barrio { get; set; }

        // En la API es int; aquí lo hacemos nullable para permitir vacío en formularios.
        [Range(1000000, 999999999, ErrorMessage = "Ingrese un teléfono válido (solo números).")]
        public int? telefono { get; set; }

        [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
        public string? redesSociales { get; set; }

        [DataType(DataType.Date)]
        public DateTime fechaIngreso { get; set; } = DateTime.Today;

        // Si en tu API fechaEgreso NO puede ser null, cámbiala a DateTime y quitá el '?'
        [DataType(DataType.Date)]
        public DateTime? fechaEgreso { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un usuario válido.")]
        public int id_usuario { get; set; }

        [DataType(DataType.Currency)]
        [Range(typeof(decimal), "0", "79228162514264337593543950335",
            ErrorMessage = "Ingrese un monto válido mayor o igual a 0.")]
        public decimal montoDia { get; set; }
    }
}
