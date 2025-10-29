using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Models;
using SantaRamona.Data; // <- ajustá si tu DbContext está en otro namespace

namespace SantaRamona.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PuntoAcopioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public PuntoAcopioController(ApplicationDbContext context) => _context = context;

        // =========================================================
        // GET: api/PuntoAcopio
        // Filtros: ?provinciaId=&localidadId=&soloActivos=&q=
        // Paginado: ?pagina=1&pageSize=20
        // =========================================================
        [HttpGet]
        public async Task<ActionResult<object>> GetAll(
            [FromQuery] int pagina = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? provinciaId = null,
            [FromQuery] int? localidadId = null,
            [FromQuery] bool? soloActivos = null,
            [FromQuery] string? q = null)
        {
            if (pagina < 1) pagina = 1;
            if (pageSize is < 1 or > 200) pageSize = 20;

            IQueryable<Punto_Acopio> query = _context.Punto_Acopio.AsNoTracking();

            if (provinciaId is > 0)
                query = query.Where(p => p.id_provincia == provinciaId);

            if (localidadId is > 0)
                query = query.Where(p => p.id_localidad == localidadId);

            if (soloActivos == true)
                query = query.Where(p => p.activo);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLower();
                query = query.Where(p =>
                    p.nombre.ToLower().Contains(q) ||
                    p.calle.ToLower().Contains(q) ||
                    (p.descripcion != null && p.descripcion.ToLower().Contains(q))
                );
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.id_puntoAcopio)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                pagina,
                pageSize,
                total,
                paginas = (int)Math.Ceiling(total / (double)pageSize),
                items
            });
        }

        // =========================================================
        // GET: api/PuntoAcopio/5
        // =========================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Punto_Acopio>> GetById(int id)
        {
            var punto = await _context.Punto_Acopio.FindAsync(id);
            if (punto is null) return NotFound();
            return Ok(punto);
        }

        // =========================================================
        // POST: api/PuntoAcopio
        // Body JSON (ejemplo):
        // { "nombre":"...", "calle":"...", "altura":123, "departamento":"2B",
        //   "id_provincia":6, "id_localidad":6028, "descripcion":"...", "activo":true }
        // =========================================================
        [HttpPost]
        public async Task<ActionResult<Punto_Acopio>> Create([FromBody] Punto_Acopio dto)
        {
            var error = Validar(dto);
            if (error is not null) return BadRequest(new { error });

            var entity = new Punto_Acopio
            {
                nombre = dto.nombre.Trim(),
                calle = dto.calle.Trim(),
                altura = dto.altura,
                departamento = string.IsNullOrWhiteSpace(dto.departamento) ? null : dto.departamento.Trim(),
                id_provincia = dto.id_provincia,
                id_localidad = dto.id_localidad,
                descripcion = string.IsNullOrWhiteSpace(dto.descripcion) ? null : dto.descripcion.Trim(),
                activo = dto.activo
            };

            _context.Punto_Acopio.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.id_puntoAcopio }, entity);
        }

        // =========================================================
        // PUT: api/PuntoAcopio/5
        // Reemplaza el recurso completo
        // =========================================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Punto_Acopio dto)
        {
            if (id != dto.id_puntoAcopio)
                return BadRequest(new { error = "El id de la ruta y del cuerpo no coinciden." });

            var error = Validar(dto);
            if (error is not null) return BadRequest(new { error });

            var entity = await _context.Punto_Acopio.FindAsync(id);
            if (entity is null) return NotFound();

            entity.nombre = dto.nombre.Trim();
            entity.calle = dto.calle.Trim();
            entity.altura = dto.altura;
            entity.departamento = string.IsNullOrWhiteSpace(dto.departamento) ? null : dto.departamento.Trim();
            entity.id_provincia = dto.id_provincia;
            entity.id_localidad = dto.id_localidad;
            entity.descripcion = string.IsNullOrWhiteSpace(dto.descripcion) ? null : dto.descripcion.Trim();
            entity.activo = dto.activo;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =========================================================
        // PATCH: api/PuntoAcopio/5/activar?valor=true
        // Cambia solo el estado "activo"
        // =========================================================
        [HttpPatch("{id:int}/activar")]
        public async Task<IActionResult> CambiarActivo(int id, [FromQuery] bool valor)
        {
            var entity = await _context.Punto_Acopio.FindAsync(id);
            if (entity is null) return NotFound();

            entity.activo = valor;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =========================================================
        // DELETE: api/PuntoAcopio/5
        // (Borrado físico; si querés, podrías hacer borrado lógico usando `activo=false`)
        // =========================================================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Punto_Acopio.FindAsync(id);
            if (entity is null) return NotFound();

            _context.Punto_Acopio.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ------------------------------ Helpers ------------------------------
        private static string? Validar(Punto_Acopio p)
        {
            if (p is null) return "Cuerpo de la solicitud inválido.";

            if (string.IsNullOrWhiteSpace(p.nombre)) return "El nombre es obligatorio.";
            if (p.nombre.Length > 50) return "El nombre no puede superar 50 caracteres.";

            if (string.IsNullOrWhiteSpace(p.calle)) return "La calle es obligatoria.";
            if (p.calle.Length > 100) return "La calle no puede superar 100 caracteres.";

            if (p.altura <= 0) return "La altura debe ser mayor a 0.";

            if (p.id_provincia <= 0) return "Debe indicar una provincia válida.";
            if (p.id_localidad <= 0) return "Debe indicar una localidad válida.";

            if (p.departamento != null && p.departamento.Length > 10)
                return "El departamento no puede superar 10 caracteres.";

            if (p.descripcion != null && p.descripcion.Length > 255)
                return "La descripción no puede superar 255 caracteres.";

            return null;
        }
    }
}
