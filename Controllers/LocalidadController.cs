using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Models;
using SantaRamona.Data; // Ajustá si tu DbContext está en otro namespace

namespace SantaRamona.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalidadController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocalidadController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Localidad
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Localidad>>> GetLocalidades()
        {
            return await _context.Localidad.ToListAsync();
        }

        // GET: api/Localidad/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Localidad>> GetLocalidad(int id)
        {
            var localidad = await _context.Localidad.FindAsync(id);

            if (localidad == null)
            {
                return NotFound();
            }

            return localidad;
        }

        // GET: api/Localidad/PorProvincia/3
        [HttpGet("PorProvincia/{id_provincia}")]
        public async Task<ActionResult<IEnumerable<Localidad>>> GetLocalidadesPorProvincia(int id_provincia)
        {
            var localidades = await _context.Localidad
                .Where(l => l.id_provincia == id_provincia)
                .OrderBy(l => l.nombre)
                .ToListAsync();

            if (!localidades.Any())
            {
                return NotFound("No hay localidades para la provincia indicada.");
            }

            return localidades;
        }

        // POST: api/Localidad
        [HttpPost]
        public async Task<ActionResult<Localidad>> PostLocalidad(Localidad localidad)
        {
            _context.Localidad.Add(localidad);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLocalidad), new { id = localidad.id_localidad }, localidad);
        }

        // PUT: api/Localidad/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocalidad(int id, Localidad localidad)
        {
            if (id != localidad.id_localidad)
            {
                return BadRequest();
            }

            _context.Entry(localidad).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocalidadExists(id))
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

        // DELETE: api/Localidad/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocalidad(int id)
        {
            var localidad = await _context.Localidad.FindAsync(id);
            if (localidad == null)
            {
                return NotFound();
            }

            _context.Localidad.Remove(localidad);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LocalidadExists(int id)
        {
            return _context.Localidad.Any(e => e.id_localidad == id);
        }
    }
}
