using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Data;
using SantaRamona.Models;

namespace SantaRamona.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RespuestaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RespuestaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Respuesta
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Respuesta>>> GetRespuestas()
        {
            return await _context.Respuesta.AsNoTracking().ToListAsync();
        }

        // GET: api/Respuesta/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Respuesta>> GetRespuesta(int id)
        {
            var r = await _context.Respuesta.FindAsync(id);
            if (r == null) return NotFound();
            return r;
        }

        // GET: api/Respuesta/por-formulario/10  (todas las respuestas de un formulario)
        [HttpGet("por-formulario/{idFormulario:int}")]
        public async Task<ActionResult<IEnumerable<Respuesta>>> GetPorFormulario(int idFormulario)
        {
            var formOk = await _context.Formulario.AnyAsync(f => f.id_formulario == idFormulario);
            if (!formOk) return NotFound($"No existe FORMULARIO {idFormulario}");

            var lista = await _context.Respuesta
                .Where(x => x.id_formulario == idFormulario)
                .AsNoTracking()
                .ToListAsync();

            return Ok(lista);
        }

        // POST: api/Respuesta
        [HttpPost]
        public async Task<ActionResult<Respuesta>> PostRespuesta(Respuesta body)
        {
            // Validaciones mínimas de FK
            var formOk = await _context.Formulario.AnyAsync(f => f.id_formulario == body.id_formulario);
            if (!formOk) return BadRequest($"id_formulario {body.id_formulario} no existe.");

            var pregOk = await _context.Pregunta.AnyAsync(p => p.id_pregunta == body.id_pregunta);
            if (!pregOk) return BadRequest($"id_pregunta {body.id_pregunta} no existe.");

            // Único lógico: una respuesta por (formulario, pregunta)
            var dup = await _context.Respuesta
                .AnyAsync(r => r.id_formulario == body.id_formulario && r.id_pregunta == body.id_pregunta);
            if (dup) return Conflict("Ya existe una respuesta para esa pregunta en ese formulario.");

            _context.Respuesta.Add(body);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRespuesta), new { id = body.id_respuesta }, body);
        }

        // PUT: api/Respuesta/5   (actualiza una respuesta puntual)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutRespuesta(int id, Respuesta body)
        {
            if (id != body.id_respuesta) return BadRequest();

            // (opcional) evitar que cambien el par formulario-pregunta en PUT
            var original = await _context.Respuesta.AsNoTracking().FirstOrDefaultAsync(r => r.id_respuesta == id);
            if (original is null) return NotFound();

            body.id_formulario = original.id_formulario;
            body.id_pregunta = original.id_pregunta;

            _context.Entry(body).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RespuestaExists(id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE: api/Respuesta/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRespuesta(int id)
        {
            var r = await _context.Respuesta.FindAsync(id);
            if (r == null) return NotFound();

            _context.Respuesta.Remove(r);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // -------------------- CARGA/EDICIÓN EN LOTE --------------------

        public record AnswerDto(int id_pregunta, string? respuesta);
        public record AnswersBatchDto(IEnumerable<AnswerDto> respuestas);

        // POST: api/Respuesta/lote/10   (upsert de respuestas por formulario)
        [HttpPost("lote/{idFormulario:int}")]
        public async Task<IActionResult> UpsertLote(int idFormulario, AnswersBatchDto body)
        {
            var form = await _context.Formulario.FirstOrDefaultAsync(f => f.id_formulario == idFormulario);
            if (form is null) return NotFound($"No existe FORMULARIO {idFormulario}");

            // (opcional) validar que las preguntas existan
            var preguntasId = body.respuestas.Select(r => r.id_pregunta).Distinct().ToList();
            var existenPreguntas = await _context.Pregunta
                .Where(p => preguntasId.Contains(p.id_pregunta))
                .Select(p => p.id_pregunta)
                .ToListAsync();

            var faltantes = preguntasId.Except(existenPreguntas).ToList();
            if (faltantes.Any())
                return BadRequest($"Preguntas inexistentes: {string.Join(", ", faltantes)}");

            // Upsert: crea si no existe, actualiza si existe
            foreach (var a in body.respuestas)
            {
                var existente = await _context.Respuesta
                    .FirstOrDefaultAsync(r => r.id_formulario == idFormulario && r.id_pregunta == a.id_pregunta);

                if (existente is null)
                {
                    _context.Respuesta.Add(new Respuesta
                    {
                        id_formulario = idFormulario,
                        id_pregunta = a.id_pregunta,
                        respuesta = a.respuesta
                    });
                }
                else
                {
                    existente.respuesta = a.respuesta;
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool RespuestaExists(int id)
            => _context.Respuesta.Any(e => e.id_respuesta == id);
    }
}