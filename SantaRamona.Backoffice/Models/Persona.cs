using System;
using System.ComponentModel.DataAnnotations;

namespace SantaRamona.Backoffice.Models
{
    public class Persona
    {
        public int id_persona { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        public string nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        public string apellido { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [Range(1000000, 99999999, ErrorMessage = "Ingrese un DNI válido.")]
        public int? dni { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime fechaNacimiento { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un email válido.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string? email { get; set; }

        [StringLength(60)]
        public string? calle { get; set; }
        
        [Range(0, 999999, ErrorMessage = "Ingrese una altura válida.")]
        public int? altura { get; set; }

        [StringLength(10)]
        public string? departamento { get; set; }

        [StringLength(60)]
        public string? barrio { get; set; }

        [StringLength(60)]
        public string? partido { get; set; }

        [StringLength(60)]
        public string? provincia { get; set; }

        [StringLength(200)]
        public string? redesSociales { get; set; }

        // Si la API setea la fecha de ingreso por defecto, podés dejarla null acá y asignarla del lado del servidor.
        public DateTime fechaIngreso { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime? fechaEgreso { get; set; }

        [StringLength(250)]
        public string? motivoEgreso { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un estado válido.")]
        public int id_estadoPersona { get; set; }
    }
}
