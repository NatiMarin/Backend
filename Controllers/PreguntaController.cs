using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Data;
using SantaRamona.Models;

namespace SantaRamona.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreguntaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PreguntaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Pregunta
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pregunta>>> GetPreguntas()
        {
            return await _context.Pregunta.AsNoTracking().ToListAsync();
        }

        // GET: api/Pregunta/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Pregunta>> GetPregunta(int id)
        {
            var p = await _context.Pregunta.FindAsync(id);
            if (p == null) return NotFound();
            return p;
        }

        // GET: api/Pregunta/por-tipo/3   (extra útil)
        [HttpGet("por-tipo/{tipoId:int}")]
        public async Task<ActionResult<IEnumerable<Pregunta>>> GetPorTipo(int tipoId)
        {
            // (opcional) validar que exista el tipo
            var tipoExiste = await _context.Tipo_Formulario.AnyAsync(t => t.id_tipoFormulario == tipoId);
            if (!tipoExiste) return NotFound($"No existe Tipo_Formulario {tipoId}");

            var lista = await _context.Pregunta
                .Where(x => x.id_tipoFormulario == tipoId)
                .AsNoTracking()
                .ToListAsync();
            return Ok(lista);
        }

        // POST: api/Pregunta
        [HttpPost]
        public async Task<ActionResult<Pregunta>> PostPregunta(Pregunta pregunta)
        {
            // (opcional) validar FK
            var tipoOk = await _context.Tipo_Formulario
                .AnyAsync(t => t.id_tipoFormulario == pregunta.id_tipoFormulario);
            if (!tipoOk) return BadRequest($"id_tipoFormulario {pregunta.id_tipoFormulario} no existe.");

            _context.Pregunta.Add(pregunta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPregunta), new { id = pregunta.id_pregunta }, pregunta);
        }

        // PUT: api/Pregunta/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutPregunta(int id, Pregunta pregunta)
        {
            if (id != pregunta.id_pregunta) return BadRequest();

            // (opcional) validar FK si cambió el tipo
            var tipoOk = await _context.Tipo_Formulario
                .AnyAsync(t => t.id_tipoFormulario == pregunta.id_tipoFormulario);
            if (!tipoOk) return BadRequest($"id_tipoFormulario {pregunta.id_tipoFormulario} no existe.");

            _context.Entry(pregunta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PreguntaExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Pregunta/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePregunta(int id)
        {
            var p = await _context.Pregunta.FindAsync(id);
            if (p == null) return NotFound();

            _context.Pregunta.Remove(p);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool PreguntaExists(int id)
            => _context.Pregunta.Any(e => e.id_pregunta == id);
    }
}