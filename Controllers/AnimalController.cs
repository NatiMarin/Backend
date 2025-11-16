using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Data;
using SantaRamona.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SantaRamona.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnimalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnimalController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========================
        // Helper para capitalizar
        // ========================
        private string? Capitalizar(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return texto;

            texto = texto.Trim().ToLower();

            var partes = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < partes.Length; i++)
            {
                var p = partes[i];
                partes[i] = char.ToUpper(p[0]) + (p.Length > 1 ? p.Substring(1) : "");
            }

            return string.Join(' ', partes);
        }

        // GET: api/animal?pagina=1&pageSize=20
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Animal>>> GetAll([FromQuery] int pagina = 1, [FromQuery] int pageSize = 20)
        {
            if (pagina < 1) pagina = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var data = await _context.Animal
                .AsNoTracking()
                .OrderByDescending(a => a.id_animal)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/animal/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Animal>> GetById(int id)
        {
            var animal = await _context.Animal.AsNoTracking().FirstOrDefaultAsync(a => a.id_animal == id);
            return animal is null ? NotFound() : Ok(animal);
        }

        // ========================
        // POST: api/animal
        // ========================
        [HttpPost]
        public async Task<ActionResult<Animal>> Create([FromBody] Animal dto)
        {
            dto.id_animal = 0;

            // Capitalización automática
            dto.nombre = Capitalizar(dto.nombre);
            dto.historia = Capitalizar(dto.historia);
            dto.seguimiento = Capitalizar(dto.seguimiento);

            if (dto.id_especie <= 0 || dto.id_estadoAnimal <= 0 || dto.id_usuario <= 0 || dto.id_tamano <= 0)
                return BadRequest("id_especie, id_estado, id_usuario e id_tamaño deben ser > 0.");

            _context.Animal.Add(dto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = dto.id_animal }, dto);
        }

        // ========================
        // PUT: api/animal/5
        // ========================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Animal dto)
        {
            if (id != dto.id_animal)
                return BadRequest("El id de la URL no coincide con el del cuerpo.");

            if (!await _context.Animal.AnyAsync(a => a.id_animal == id))
                return NotFound();

            // Capitalización automática
            dto.nombre = Capitalizar(dto.nombre);
            dto.historia = Capitalizar(dto.historia);
            dto.seguimiento = Capitalizar(dto.seguimiento);

            // NUEVO: fecha de modificación automática en cada update
            dto.fechaModificacion = DateTime.Now;

            _context.Entry(dto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Animal.AnyAsync(a => a.id_animal == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        // DELETE: api/animal/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Animal.FindAsync(id);
            if (entity is null) return NotFound();

            _context.Animal.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
