using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SantaRamona.Data;
using SantaRamona.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SantaRamona.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PensionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PensionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===================== GET (Paginado) =====================
        // GET: api/pension?pagina=1&pageSize=20
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pension>>> GetAll([FromQuery] int pagina = 1, [FromQuery] int pageSize = 20)
        {
            if (pagina < 1) pagina = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var data = await _context.Pension
                .AsNoTracking()
                .OrderByDescending(p => p.id_pension)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(data);
        }

        // ===================== GET por ID =====================
        // GET: api/pension/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Pension>> GetById(int id)
        {
            var pension = await _context.Pension
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.id_pension == id);

            return pension is null ? NotFound() : Ok(pension);
        }

        // ===================== POST =====================
        // POST: api/pension
        [HttpPost]
        public async Task<ActionResult<Pension>> Create([FromBody] Pension dto)
        {
            // El id lo maneja la DB
            dto.id_pension = 0;

            // ---------- Validaciones básicas ----------
            var msg = Validar(dto, isUpdate: false);
            if (msg is not null) return BadRequest(msg);

            if (dto.fechaIngreso == default)
                dto.fechaIngreso = DateTime.Now;

            _context.Pension.Add(dto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = dto.id_pension }, dto);
        }

        // ===================== PUT =====================
        // PUT: api/pension/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Pension dto)
        {
            if (id != dto.id_pension)
                return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var exists = await _context.Pension.AnyAsync(p => p.id_pension == id);
            if (!exists) return NotFound();

            var msg = Validar(dto, isUpdate: true);
            if (msg is not null) return BadRequest(msg);

            _context.Entry(dto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Pension.AnyAsync(p => p.id_pension == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // ===================== DELETE =====================

        [HttpPut("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id, [FromBody] EliminarPensionDto dto)
        {
            var entity = await _context.Pension.FindAsync(id);
            if (entity is null)
                return NotFound();

            entity.fechaEliminacion = dto.fechaEliminacion ?? DateTime.Now;
            entity.id_usuario = dto.id_usuario;

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Pensión eliminada correctamente." });
        }

        // ===================== Helpers =====================
        private static string? Validar(Pension p, bool isUpdate)
        {
            // telefono1 (obligatorio y mínimo 6 dígitos/char)
            if (string.IsNullOrWhiteSpace(p.telefono1) || p.telefono1.Trim().Length < 6)
                return "El campo 'telefono1' es obligatorio y debe tener al menos 6 caracteres.";

            // calle (obligatorio)
            if (string.IsNullOrWhiteSpace(p.calle))
                return "El campo 'calle' es obligatorio.";

            // altura (>= 0)
            if (p.altura < 0)
                return "El campo 'altura' debe ser mayor o igual a 0.";

            // id_provincia / id_localidad / id_estadoPension / id_usuario (> 0)
            if (p.id_provincia <= 0) return "Debe indicar una provincia válida.";
            if (p.id_localidad <= 0) return "Debe indicar una localidad válida.";
            if (p.id_estadoPension <= 0) return "Debe indicar un estado de pensión válido.";
            if (p.id_usuario <= 0) return "Debe indicar un usuario válido.";

            // email (si viene, validar formato)
            if (!string.IsNullOrWhiteSpace(p.email))
            {
                var attr = new EmailAddressAttribute();
                if (!attr.IsValid(p.email))
                    return "El formato de 'email' no es válido.";
            }

            // montoDia (si viene, >= 0)
            if (p.montoDia.HasValue && p.montoDia.Value < 0)
                return "El campo 'montoDia' no puede ser negativo.";

            // fechaEgreso (si viene, no anterior a ingreso)
            if (p.fechaEgreso.HasValue && p.fechaEgreso.Value < (p.fechaIngreso == default ? DateTime.Now : p.fechaIngreso))
                return "La 'fechaEgreso' no puede ser anterior a 'fechaIngreso'.";

            return null;
        }
        public class EliminarPensionDto
        {
            public DateTime? fechaEliminacion { get; set; }
            public int id_usuario { get; set; }
        }
    }
}
