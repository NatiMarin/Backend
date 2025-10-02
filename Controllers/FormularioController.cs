using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Data;        // Tu DbContext (ApplicationDbContext)
using SantaRamona.Models;      // Tu entity Formulario (opcional si la tenés en otro namespace)

namespace SantaRamona.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormularioController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public FormularioController(ApplicationDbContext db) => _db = db;

        // ========= DTOs =========
        public record FormCreateDto(int id_persona, int id_tipoFormulario, int? id_usuario, int id_estadoFormulario);
        public record FormUpdateDto(int id_persona, int id_tipoFormulario, int? id_usuario, int id_estadoFormulario);
        public record CambiarEstadoDto(int id_estadoFormulario);

        // ========= GET: listar con filtros y paginado =========
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> Get(
            [FromQuery] int? personaId,
            [FromQuery] int? tipoId,
            [FromQuery] int? estadoId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;

            // ⚠️ Asegurate que en tu DbContext exista DbSet<Formulario> Formularios
            var q = _db.Formulario
                .AsNoTracking()
                .AsQueryable();

            if (personaId is not null) q = q.Where(f => f.id_persona == personaId);
            if (tipoId is not null) q = q.Where(f => f.id_tipoFormulario == tipoId);
            if (estadoId is not null) q = q.Where(f => f.id_estadoFormulario == estadoId);

            var data = await q
                .OrderByDescending(f => f.fechaAltaFormulario)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new
                {
                    f.id_formulario,
                    f.id_persona,
                    f.id_tipoFormulario,
                    f.id_usuario,
                    f.id_estadoFormulario,
                    f.fechaAltaFormulario
                })
                .ToListAsync();

            return Ok(data);
        }

        // ========= GET: detalle por id =========
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var f = await _db.Formulario.AsNoTracking().FirstOrDefaultAsync(x => x.id_formulario == id);
            if (f is null) return NotFound();

            return Ok(new
            {
                f.id_formulario,
                f.id_persona,
                f.id_tipoFormulario,
                f.id_usuario,
                f.id_estadoFormulario,
                f.fechaAltaFormulario
            });
        }

        // ========= POST: crear =========
        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] FormCreateDto dto)
        {
            // Si tu tabla usa DEFAULT GETDATE(), no seteamos fechaAltaFormulario (lo pone SQL Server)
            var entity = new Formulario
            {
                id_persona = dto.id_persona,
                id_tipoFormulario = dto.id_tipoFormulario,
                id_usuario = dto.id_usuario,
                id_estadoFormulario = dto.id_estadoFormulario
                // fechaAltaFormulario => lo setea la DB
            };

            _db.Formulario.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = entity.id_formulario },
                new
                {
                    entity.id_formulario,
                    entity.id_persona,
                    entity.id_tipoFormulario,
                    entity.id_usuario,
                    entity.id_estadoFormulario,
                    entity.fechaAltaFormulario
                });
        }

        // ========= PUT: actualizar todos los campos editables =========
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] FormUpdateDto dto)
        {
            var f = await _db.Formulario.FirstOrDefaultAsync(x => x.id_formulario == id);
            if (f is null) return NotFound();

            f.id_persona = dto.id_persona;
            f.id_tipoFormulario = dto.id_tipoFormulario;
            f.id_usuario = dto.id_usuario;
            f.id_estadoFormulario = dto.id_estadoFormulario;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ========= DELETE: (opcional) borrar =========
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var f = await _db.Formulario.FirstOrDefaultAsync(x => x.id_formulario == id);
            if (f is null) return NotFound();

            _db.Formulario.Remove(f);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
