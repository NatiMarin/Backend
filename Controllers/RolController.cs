using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Data;
using SantaRamona.Models;
using SantaRamona.Models.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SantaRamona.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RolController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===========================
        // GET: api/rol
        // Lista completa de roles
        // ===========================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rol>>> GetAll()
        {
            var roles = await _context.Rol
                .AsNoTracking()
                .OrderBy(r => r.id_rol)
                .ToListAsync();

            return Ok(roles);
        }

        // =========================================
        // GET: api/rol/select
        // Versión liviana (ideal para combos)
        // =========================================
        [HttpGet("select")]
        public async Task<ActionResult<IEnumerable<RolDto>>> GetForSelect()
        {
            var roles = await _context.Rol
                .AsNoTracking()
                .OrderBy(r => r.descripcion)
                .Select(r => new RolDto(r.id_rol, r.descripcion))
                .ToListAsync();

            return Ok(roles);
        }

        // ===========================
        // GET: api/rol/{id}
        // Rol por id
        // ===========================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Rol>> GetById(int id)
        {
            var rol = await _context.Rol
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.id_rol == id);

            return rol is null ? NotFound() : Ok(rol);
        }

        // ===========================
        // POST: api/rol
        // Crear rol
        // ===========================
        [HttpPost]
        public async Task<ActionResult<Rol>> Create([FromBody] Rol rol)
        {
            // Validación mínima
            if (string.IsNullOrWhiteSpace(rol.descripcion))
                return BadRequest("La descripción es obligatoria.");

            _context.Rol.Add(rol);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = rol.id_rol }, rol);
        }

        // ===========================
        // PUT: api/rol/{id}
        // Actualizar rol
        // ===========================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Rol rol)
        {
            if (id != rol.id_rol)
                return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var exists = await _context.Rol.AnyAsync(r => r.id_rol == id);
            if (!exists) return NotFound();

            _context.Entry(rol).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Rol.AnyAsync(r => r.id_rol == id))
                    return NotFound();
                throw;
            }
        }

        // ===========================
        // DELETE: api/rol/{id}
        // Eliminar rol
        // ===========================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rol = await _context.Rol.FindAsync(id);
            if (rol is null) return NotFound();

            _context.Rol.Remove(rol);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ============================================================
        // (Opcional) Si te sirve: usuarios que poseen un rol concreto
        // GET: api/rol/{id}/usuarios
        // Útil para auditoría o para mostrar desde el rol hacia usuarios.
        // ============================================================
        [HttpGet("{id:int}/usuarios")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsuariosConRol(int id)
        {
            var existeRol = await _context.Rol.AnyAsync(r => r.id_rol == id);
            if (!existeRol) return NotFound($"No existe rol con id {id}");

            var usuarios = await _context.Usuario_Rol
                .Where(ur => ur.id_rol == id)
                .Select(ur => new
                {
                    ur.Usuario!.id_usuario,
                    ur.Usuario.nombre,
                    ur.Usuario.apellido,
                    ur.Usuario.email
                })
                .ToListAsync();

            return Ok(usuarios);
        }
    }
}
