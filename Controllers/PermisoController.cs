using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Models;
using SantaRamona.Data;

namespace SantaRamona.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermisoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PermisoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Permiso
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permiso>>> GetPermisos()
        {
            return await _context.Permiso.ToListAsync();
        }

        // GET: api/Permiso/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permiso>> GetPermiso(int id)
        {
            var permiso = await _context.Permiso.FindAsync(id);
            if (permiso == null)
            {
                return NotFound();
            }
            return permiso;
        }

        // POST: api/Permiso
        [HttpPost]
        public async Task<ActionResult<Permiso>> PostPermiso(Permiso permiso)
        {
            _context.Permiso.Add(permiso);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPermiso), new { id = permiso.id_permiso }, permiso);
        }

        // PUT: api/Permiso/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermiso(int id, Permiso permiso)
        {
            if (id != permiso.id_permiso)
            {
                return BadRequest();
            }

            _context.Entry(permiso).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PermisoExists(id))
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

        // DELETE: api/Permiso/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermiso(int id)
        {
            var permiso = await _context.Permiso.FindAsync(id);
            if (permiso == null)
            {
                return NotFound();
            }

            _context.Permiso.Remove(permiso);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PermisoExists(int id)
        {
            return _context.Permiso.Any(e => e.id_permiso == id);
        }
    }
}