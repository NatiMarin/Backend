using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Models;
using SantaRamona.Data;

namespace SantaRamona.Controllers
{
    [Route("api/Estado_Usuario")]
    [ApiController]
    public class EstadoUsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EstadoUsuarioController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var estados = await _context.Estado_Usuario
                .AsNoTracking()
                .Select(e => new
                {
                    e.id_estadoUsuario,
                    e.descripcion,
                    enUso = _context.Usuario.Any(u => u.id_estadoUsuario == e.id_estadoUsuario)
                })
                .ToListAsync();

            return Ok(estados);
        }
       

        // GET: api/Estado_Usuario/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Estado_Usuario>> GetEstado_Usuario(int id)
        {
            var estado_usuario = await _context.Estado_Usuario.FindAsync(id);
            if (estado_usuario == null)
            {
                return NotFound();
            }
            return estado_usuario;
        }

        // POST: api/Estado_Usuario
        [HttpPost]
        public async Task<ActionResult<Estado_Usuario>> PostEstado_Usuario(Estado_Usuario Estado_Usuario)
        {
            _context.Estado_Usuario.Add(Estado_Usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEstado_Usuario), new { id = Estado_Usuario.id_estadoUsuario }, Estado_Usuario);
        }

        // PUT: api/Estado_Usuario/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEstado_Usuario(int id, Estado_Usuario Estado_Usuario)
        {
            if (id != Estado_Usuario.id_estadoUsuario)
            {
                return BadRequest();
            }

            _context.Entry(Estado_Usuario).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Estado_UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // DELETE: api/Estado_Usuario/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEstado_Usuario(int id)
        {
            var entity = await _context.Estado_Usuario.FindAsync(id);
            if (entity is null)
                return NotFound("El estado no existe o ya fue eliminado.");

            // 🔹 Verificar si algún usuario lo usa
            bool enUso = await _context.Usuario.AnyAsync(u => u.id_estadoUsuario == id);
            if (enUso)
            {
                // ⚠️ Devuelve un mensaje amigable (sin error técnico)
                return Conflict("No puedes eliminar este estado porque está siendo usado por un usuario. Podés corregir los datos desde la sección Modificar.");
            }

            _context.Estado_Usuario.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool Estado_UsuarioExists(int id)
        {
            return _context.Estado_Usuario.Any(e => e.id_estadoUsuario == id);
        }
    }
}