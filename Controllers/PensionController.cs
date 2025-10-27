using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Data;
using SantaRamona.Models;

namespace SantaRamona.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PensionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public PensionController(ApplicationDbContext context) => _context = context;

        // =========================================================
        // GET: api/pension?pagina=1&pageSize=20&estadoId=&provinciaId=&localidadId=&soloActivas=&q=
        // Listado con paginado y filtros básicos
        // =========================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pension>>> GetAll(
            [FromQuery] int pagina = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? estadoId = null,
            [FromQuery] int? provinciaId = null,
            [FromQuery] int? localidadId = null,
            [FromQuery] bool? soloActivas = null,
            [FromQuery] string? q = null)
        {
            if (pagina < 1) pagina = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            IQueryable<Pension> query = _context.Pension.AsNoTracking();

            if (estadoId.HasValue && estadoId.Value > 0)
                query = query.Where(p => p.id_estadoPension == estadoId.Value);

            if (provinciaId.HasValue && provinciaId.Value > 0)
                query = query.Where(p => p.id_provincia == provinciaId.Value);

            if (localidadId.HasValue && localidadId.Value > 0)
                query = query.Where(p => p.id_localidad == localidadId.Value);

            if (soloActivas.HasValue && soloActivas.Value)
                query = query.Where(p => p.fechaEgreso == null);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                query = query.Where(p =>
                    (p.nombre ?? "").ToLower().Contains(term) ||
                    (p.email ?? "").ToLower().Contains(term) ||
                    (p.telefono1 ?? "").ToLower().Contains(term) ||
                    (p.telefono2 ?? "").ToLower().Contains(term) ||
                    (p.redesSociales ?? "").ToLower().Contains(term));
            }

            var data = await query
                .OrderBy(p => p.id_pension)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(data);
        }

        // ===========================
        // GET: api/pension/5
        // ===========================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Pension>> GetById(int id)
        {
            var entity = await _context.Pension.AsNoTracking()
                .FirstOrDefaultAsync(p => p.id_pension == id);

            return entity is null ? NotFound() : Ok(entity);
        }

        // ===========================
        // POST: api/pension
        // Crear
        // ===========================
        [HttpPost]
        public async Task<ActionResult<Pension>> Create([FromBody] Pension dto)
        {
            // Validaciones mínimas de campos obligatorios
            if (string.IsNullOrWhiteSpace(dto.calle))
                return BadRequest("El campo 'calle' es obligatorio.");

            if (dto.altura <= 0)
                return BadRequest("El campo 'altura' debe ser mayor a 0.");

            if (string.IsNullOrWhiteSpace(dto.telefono1))
                return BadRequest("El campo 'telefono1' es obligatorio.");

            if (dto.id_provincia <= 0 || dto.id_localidad <= 0)
                return BadRequest("Debe indicar una provincia y localidad válidas.");

            if (dto.id_estadoPension <= 0)
                return BadRequest("Debe indicar un estado de pensión válido.");

            if (dto.id_usuario <= 0)
                return BadRequest("Debe indicar un usuario válido.");

            // Chequeo de FKs (si tus DbSet existen en el DbContext)
            var fkOk = await FksValidas(dto);
            if (!fkOk.ok)
                return BadRequest(fkOk.mensaje);

            // Reseteo por si viene seteado
            dto.id_pension = 0;

            // Si no viene fecha, usar DateTime.Now (tu modelo ya tiene default)
            if (dto.fechaIngreso == default)
                dto.fechaIngreso = DateTime.Now;

            _context.Pension.Add(dto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = dto.id_pension }, dto);
        }

        // ===========================
        // PUT: api/pension/5
        // Actualizar
        // ===========================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Pension dto)
        {
            if (id != dto.id_pension)
                return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var exists = await _context.Pension.AnyAsync(p => p.id_pension == id);
            if (!exists) return NotFound();

            // Validaciones mínimas
            if (string.IsNullOrWhiteSpace(dto.calle))
                return BadRequest("El campo 'calle' es obligatorio.");

            if (dto.altura <= 0)
                return BadRequest("El campo 'altura' debe ser mayor a 0.");

            if (string.IsNullOrWhiteSpace(dto.telefono1))
                return BadRequest("El campo 'telefono1' es obligatorio.");

            if (dto.id_provincia <= 0 || dto.id_localidad <= 0)
                return BadRequest("Debe indicar una provincia y localidad válidas.");

            if (dto.id_estadoPension <= 0)
                return BadRequest("Debe indicar un estado de pensión válido.");

            if (dto.id_usuario <= 0)
                return BadRequest("Debe indicar un usuario válido.");

            var fkOk = await FksValidas(dto);
            if (!fkOk.ok)
                return BadRequest(fkOk.mensaje);

            _context.Entry(dto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Pension.AnyAsync(p => p.id_pension == id))
                    return NotFound();
                throw;
            }
        }

        // ===========================
        // DELETE: api/pension/5
        // ===========================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Pension.FindAsync(id);
            if (entity is null) return NotFound();

            // Si necesitás reglas de negocio (no eliminar activas, etc.), aplicalas acá.
            _context.Pension.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ============================================================
        // EXTRA ÚTILES
        // ============================================================

        // Marcar egreso (setea fechaEgreso = ahora)
        // PUT: api/pension/5/egreso
        [HttpPut("{id:int}/egreso")]
        public async Task<IActionResult> MarcarEgreso(int id)
        {
            var entity = await _context.Pension.FindAsync(id);
            if (entity is null) return NotFound();

            if (entity.fechaEgreso != null)
                return BadRequest("La pensión ya tiene fecha de egreso.");

            entity.fechaEgreso = DateTime.Now;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Cambiar monto día
        // PUT: api/pension/5/monto/{valor}
        [HttpPut("{id:int}/monto/{valor:decimal}")]
        public async Task<IActionResult> CambiarMontoDia(int id, decimal valor)
        {
            if (valor < 0) return BadRequest("El monto no puede ser negativo.");

            var entity = await _context.Pension.FindAsync(id);
            if (entity is null) return NotFound();

            entity.montoDia = valor;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ============================================================
        // Helpers internos
        // ============================================================
        private async Task<(bool ok, string mensaje)> FksValidas(Pension p)
        {
            // Chequeos defensivos, asumen que tenés estas tablas en tu DbContext
            if (!await _context.Provincia.AnyAsync(x => x.id_provincia == p.id_provincia))
                return (false, "La provincia indicada no existe.");

            if (!await _context.Localidad.AnyAsync(x => x.id_localidad == p.id_localidad))
                return (false, "La localidad indicada no existe.");

            if (!await _context.Estado_Pension.AnyAsync(x => x.id_estadoPension == p.id_estadoPension))
                return (false, "El estado de pensión indicado no existe.");

            if (!await _context.Usuario.AnyAsync(x => x.id_usuario == p.id_usuario))
                return (false, "El usuario indicado no existe.");

            return (true, string.Empty);
        }
    }
}
