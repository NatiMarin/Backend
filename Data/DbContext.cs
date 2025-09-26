using Microsoft.EntityFrameworkCore;
using SantaRamona.Models;
using System.Collections.Generic;

namespace SantaRamona.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Raza> Raza { get; set; }
        public DbSet<Especie> Especie { get; set; }
        public DbSet<Estado> Estado { get; set; }
        public DbSet<Estado_Usuario> Estado_Usuario { get; set; }
        public DbSet<Permiso> Permiso { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<Tamano> Tamano { get; set; }
    }
}