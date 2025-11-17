using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Models;
using SantaRamona.Data;

namespace SantaRamona.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspecieController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EspecieController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========================
        // Helper para normalizar nombre de especie
        // ========================
        private string? NormalizarEspecie(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return texto;

            texto = texto.Trim().ToLower();

            // Primera letra en mayúscula, resto minúsculas
            return char.ToUpper(texto[0]) + (texto.Length > 1 ? texto.Substring(1) : "");
        }

        // GET: api/Especie
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Especie>>> GetEspecie()
        {
            return await _context.Especie.ToListAsync();
        }

        // GET: api/especie/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Especie>> GetEspecie(int id)
        {
            var especie = await _context.Especie.FindAsync(id);
            if (especie == null)
            {
                return NotFound();
            }
            return especie;
        }

        // POST: api/especie
        [HttpPost]
        public async Task<ActionResult<Especie>> PostEspecie(Especie especie)
        {
            // Normalizar nombre de especie
            especie.especie = NormalizarEspecie(especie.especie);

            _context.Especie.Add(especie);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEspecie), new { id = especie.id_especie }, especie);
        }

        // PUT: api/especie/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEspecie(int id, Especie especie)
        {
            if (id != especie.id_especie)
            {
                return BadRequest();
            }

            // Normalizar nombre de especie también al modificar
            especie.especie = NormalizarEspecie(especie.especie);

            _context.Entry(especie).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EspecieExists(id))
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

        // DELETE: api/especie/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEspecie(int id)
        {
            var especie = await _context.Especie.FindAsync(id);
            if (especie == null)
            {
                return NotFound();
            }

            _context.Especie.Remove(especie);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EspecieExists(int id)
        {
            return _context.Especie.Any(e => e.id_especie == id);
        }
    }
}