namespace SantaRamona.Models
{
    public class Usuario
    {
        public int id_usuario { get; set; }
        public string clave { get; set; }
        public string email { get; set; }
        public string nombre { get; set; }
        public string id_rol {  get; set; }
        public string calle { get; set; }
        public string altura { get; set; }
        public string pisoCasa { get; set; }
        public string departamento { get; set; }
        public string barrio { get; set; }
        public int telefono { get; set; }
        public DateTime fechaRegistro { get; set; }
        public int id_estadoUsuario { get; set; }


    }
}
