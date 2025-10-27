// SantaRamona.Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using SantaRamona.Models;

namespace SantaRamona.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Raza> Raza { get; set; }
        public DbSet<Especie> Especie { get; set; }
        public DbSet<Estado_Animal> Estado_Animal { get; set; }
        public DbSet<Estado_Usuario> Estado_Usuario { get; set; }
        public DbSet<Estado_Persona> Estado_Persona { get; set; }
        public DbSet<Estado_Formulario> Estado_Formulario { get; set; }
        public DbSet<Estado_Pension> Estado_Pension { get; set; }
        public DbSet<Tipo_Telefono> Tipo_Telefono { get; set; }
        public DbSet<Tipo_Formulario> Tipo_Formulario { get; set; }
        public DbSet<Permiso> Permiso { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<Usuario_Rol> Usuario_Rol { get; set; }
        public DbSet<Tamano> Tamano { get; set; }
        public DbSet<Animal> Animal { get; set; }
        public DbSet<Historial_Medico> Historial_Medico { get; set; }
        public DbSet<Persona> Persona { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Pension> Pension { get; set; }
        public DbSet<Provincia> Provincia { get; set; }
        public DbSet<Localidad> Localidad { get; set; }
        public DbSet<Seguimiento> Seguimiento { get; set; }
        public DbSet<Formulario> Formulario { get; set; }
        public DbSet<Pregunta> Pregunta { get; set; }
        public DbSet<Respuesta> Respuesta { get; set; }
        public DbSet<Donacion> Donacion { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === Mapear tablas exactas (explícito y prolijo) ===
            modelBuilder.Entity<Usuario>().ToTable("USUARIO");
            modelBuilder.Entity<Rol>().ToTable("ROL");
            modelBuilder.Entity<Usuario_Rol>().ToTable("USUARIO_ROL");

            // === Clave compuesta y FKs de USUARIO_ROL ===
            modelBuilder.Entity<Usuario_Rol>(entity =>
            {
                entity.HasKey(ur => new { ur.id_usuario, ur.id_rol });

                entity.HasOne(ur => ur.Usuario)
                      .WithMany() // si luego agregás colecciones, cámbialo por .WithMany(u => u.UsuarioRoles)
                      .HasForeignKey(ur => ur.id_usuario)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Rol)
                      .WithMany()
                      .HasForeignKey(ur => ur.id_rol)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Reglas mínimas (opcional, pero útil)
            modelBuilder.Entity<Usuario>(e =>
            {
                e.Property(x => x.email).IsRequired();
                e.Property(x => x.clave).IsRequired();
            });
            modelBuilder.Entity<Rol>(e =>
            {
                e.Property(x => x.descripcion).IsRequired();
            });
        }
    }
}
