using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Models;
using SantaRamona.Data;

namespace SantaRamona.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Estado_UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public Estado_UsuarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Estado_Usuario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Estado_Usuario>>> GetEstado_Usuarios()
        {
            return await _context.Estado_Usuario.ToListAsync();
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
            var estado_usuario = await _context.Estado_Usuario.FindAsync(id);
            if (estado_usuario == null)
            {
                return NotFound();
            }

            _context.Estado_Usuario.Remove(estado_usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool Estado_UsuarioExists(int id)
        {
            return _context.Estado_Usuario.Any(e => e.id_estadoUsuario == id);
        }
    }
}