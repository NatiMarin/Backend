// SantaRamona.Controllers/UsuarioController.cs
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
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UsuarioController(ApplicationDbContext context) => _context = context;

        [HttpPut("{id:int}/rol/{idRol:int}")]
        public async Task<IActionResult> SetRolUnico(int id, int idRol)
        {
            var existeUsuario = await _context.Usuario.AnyAsync(u => u.id_usuario == id);
            if (!existeUsuario) return NotFound("El usuario no existe.");

            var existeRol = await _context.Rol.AnyAsync(r => r.id_rol == idRol);
            if (!existeRol) return BadRequest("El rol indicado no existe.");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var actuales = _context.Usuario_Rol.Where(ur => ur.id_usuario == id);
                _context.Usuario_Rol.RemoveRange(actuales);
                await _context.SaveChangesAsync();

                _context.Usuario_Rol.Add(new Usuario_Rol { id_usuario = id, id_rol = idRol });
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, $"Error al asignar rol: {ex.Message}");
            }
        }

        // === NUEVO: Roles del usuario ===
        // GET: api/usuario/{id}/roles
        [HttpGet("{id:int}/roles")]
        public async Task<ActionResult<IEnumerable<RolDto>>> GetRolesByUsuario(int id)
        {
            var existe = await _context.Usuario.AnyAsync(u => u.id_usuario == id);
            if (!existe) return NotFound($"No existe usuario con id {id}");

            var roles = await _context.Usuario_Rol
                .Where(ur => ur.id_usuario == id)
                .Select(ur => new RolDto(ur.Rol!.id_rol, ur.Rol.descripcion))
                .ToListAsync();

            return Ok(roles);
        }

        // === NUEVO: Roles NO asignados al usuario (útil para combo “agregar rol”) ===
        // GET: api/usuario/{id}/roles/disponibles
        [HttpGet("{id:int}/roles/disponibles")]
        public async Task<ActionResult<IEnumerable<RolDto>>> GetRolesNoAsignados(int id)
        {
            var asignados = _context.Usuario_Rol
                .Where(ur => ur.id_usuario == id)
                .Select(ur => ur.id_rol);

            var disponibles = await _context.Rol
                .Where(r => !asignados.Contains(r.id_rol))
                .Select(r => new RolDto(r.id_rol, r.descripcion))
                .ToListAsync();

            return Ok(disponibles);
        }

        // === lo tuyo existente ===
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetAll([FromQuery] int pagina = 1, [FromQuery] int pageSize = 20)
        {
            if (pagina < 1) pagina = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var data = await _context.Usuario
                .AsNoTracking()
                .OrderBy(u => u.id_usuario)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Usuario>> GetById(int id)
        {
            var usuario = await _context.Usuario
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.id_usuario == id);

            return usuario is null ? NotFound() : Ok(usuario);
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> Create([FromBody] Usuario dto)
        {
            dto.id_usuario = 0;

            if (string.IsNullOrWhiteSpace(dto.email) || string.IsNullOrWhiteSpace(dto.clave))
                return BadRequest("El email y la clave son obligatorios.");

            if (dto.id_estadoUsuario <= 0)
                return BadRequest("id_estadoUsuario debe ser mayor a 0.");

            _context.Usuario.Add(dto);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UQ__USUARIO") == true)
            {
                return BadRequest("El email ya está registrado. Ingrese otro diferente.");
            }

            return CreatedAtAction(nameof(GetById), new { id = dto.id_usuario }, dto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Usuario dto)
        {
            if (id != dto.id_usuario)
                return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var exists = await _context.Usuario.AnyAsync(u => u.id_usuario == id);
            if (!exists) return NotFound();

            _context.Entry(dto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Usuario.AnyAsync(u => u.id_usuario == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Usuario.FindAsync(id);
            if (entity is null) return NotFound();

            _context.Usuario.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
