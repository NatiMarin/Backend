using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Data;
using SantaRamona.Models;
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

        // GET: api/usuario?pagina=1&pageSize=20
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetAll([FromQuery] int pagina = 1, [FromQuery] int pageSize = 20)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message} | Inner: {ex.InnerException?.Message}");
            }
        }


        // GET: api/usuario/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Usuario>> GetById(int id)
        {
            var usuario = await _context.Usuario
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.id_usuario == id);

            return usuario is null ? NotFound() : Ok(usuario);
        }

        // POST: api/usuario
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

        // PUT: api/usuario/5
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

        // DELETE: api/usuario/5
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
