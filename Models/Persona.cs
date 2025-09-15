namespace SantaRamona.Models
{
    public class Persona
    {
        public int id_persona { get; set; }
        public string nombre { get; set; }
        public int dni { get; set;}
        public string email { get; set; }
        public string calle { get; set; }
        public string altura { get; set; }
        public string pisoCasa { get; set; }
        public string departamento { get; set; }
        public string barrio { get; set; }
        public int telefono { get; set; }
        public string redesSociales { get; set; }
        public DateTime fechaIngreso { get; set; }
        public DateTime fechaEgreso { get; set; }
        public string motivoEgreso { get; set; }
        public int id_usuario { get; set; }




    }
}
