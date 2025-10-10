using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SantaRamona.Backoffice.Models;
using System.Text;
using System.Text.Json;

namespace SantaRamona.Backoffice.Controllers
{
    public class PersonaController : Controller
    {
        private readonly IHttpClientFactory _http;
        public PersonaController(IHttpClientFactory http) => _http = http;

        // Rutas API (ajusta si tu swagger muestra otra cosa)
        private const string RUTA_PERSONA = "/api/persona";
        private const string RUTA_ESTADO_PERSONA = "/api/EstadoPersona"; // <-- verifica tu ruta real

        private static readonly JsonSerializerOptions JsonOps = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // ===== Helpers de estados =====
        private async Task<SelectList> CargarEstadosSelectAsync(HttpClient client, int? seleccionado = null)
        {
            var resp = await client.GetAsync(RUTA_ESTADO_PERSONA);
            if (!resp.IsSuccessStatusCode)
            {
                if (resp.StatusCode != System.Net.HttpStatusCode.NotFound)
                    ViewBag.ApiError = $"No se pudieron cargar los estados ({(int)resp.StatusCode})";
                return new SelectList(Enumerable.Empty<SelectListItem>());
            }

            var json = await resp.Content.ReadAsStringAsync();
            var lista = JsonSerializer.Deserialize<IEnumerable<Estado_Persona>>(json, JsonOps)
                        ?? Enumerable.Empty<Estado_Persona>();

            var dict = lista.ToDictionary(e => e.id_estadoPersona, e => e.descripcion);
            return new SelectList(dict, "Key", "Value", seleccionado);
        }

        private async Task<Dictionary<int, string>> CargarEstadosDictAsync(HttpClient client)
        {
            var dict = new Dictionary<int, string>();
            var resp = await client.GetAsync(RUTA_ESTADO_PERSONA);
            if (!resp.IsSuccessStatusCode) return dict;

            var json = await resp.Content.ReadAsStringAsync();
            var lista = JsonSerializer.Deserialize<IEnumerable<Estado_Persona>>(json, JsonOps)
                        ?? Enumerable.Empty<Estado_Persona>();

            return lista.ToDictionary(e => e.id_estadoPersona, e => e.descripcion);
        }

        // ===== Index =====
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _http.CreateClient("Api");

            // 1) Personas
            var respPersonas = await client.GetAsync(RUTA_PERSONA);
            if (!respPersonas.IsSuccessStatusCode)
            {
                var body = await respPersonas.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"GET {RUTA_PERSONA} -> {(int)respPersonas.StatusCode} {respPersonas.ReasonPhrase}. {body}";
                ViewBag.Estados = new Dictionary<int, string>();
                return View(Enumerable.Empty<Persona>());
            }

            var personasJson = await respPersonas.Content.ReadAsStringAsync();
            var personas = JsonSerializer.Deserialize<IEnumerable<Persona>>(personasJson, JsonOps)
                           ?? Enumerable.Empty<Persona>();

            // 2) Estados (diccionario id -> descripción)
            var respEstados = await client.GetAsync(RUTA_ESTADO_PERSONA);
            var estadosDict = new Dictionary<int, string>();
            if (respEstados.IsSuccessStatusCode)
            {
                var json = await respEstados.Content.ReadAsStringAsync();
                var lista = JsonSerializer.Deserialize<IEnumerable<Estado_Persona>>(json, JsonOps)
                            ?? Enumerable.Empty<Estado_Persona>();
                estadosDict = lista.ToDictionary(e => e.id_estadoPersona, e => e.descripcion);
            }
            ViewBag.Estados = estadosDict;

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
            if (TempData["Error"] is string err) ViewBag.Error = err;

            return View(personas);
        }

        // ===== Detalle =====
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var client = _http.CreateClient("Api");

            var resp = await client.GetAsync($"{RUTA_PERSONA}/{id}");
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = $"No se pudo obtener la persona {id}. {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
                return RedirectToAction(nameof(Index));
            }

            var persona = JsonSerializer.Deserialize<Persona>(body, JsonOps);

            // Para selects (si en algún partial lo usás)
            ViewBag.EstadosSL = await CargarEstadosSelectAsync(client, persona?.id_estadoPersona);
            // Para mostrar la descripción en la vista
            ViewBag.Estados = await CargarEstadosDictAsync(client);

            // Si la petición es AJAX (modal), podés devolver un partial:
            // if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            //     return PartialView("_DetallePersona", persona);

            return View(persona);
        }

        // ===== Crear =====
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var client = _http.CreateClient("Api");
            ViewBag.Estados = await CargarEstadosSelectAsync(client);
            return View(new Persona { fechaIngreso = DateTime.Today }); // pre-cargar hoy
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([FromForm] Persona persona)
        {
            // Forzar fecha del día (si no querés permitir setearla manualmente)
            persona.fechaIngreso = DateTime.Today;

            if (!ModelState.IsValid)
            {
                ViewBag.Estados = await CargarEstadosSelectAsync(_http.CreateClient("Api"), persona.id_estadoPersona);
                return View(persona);
            }

            var client = _http.CreateClient("Api");
            var content = new StringContent(JsonSerializer.Serialize(persona), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(RUTA_PERSONA, content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"POST {RUTA_PERSONA} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
                ViewBag.Estados = await CargarEstadosSelectAsync(client, persona.id_estadoPersona);
                return View(persona);
            }

            TempData["Ok"] = "Persona creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ===== Modificar =====
        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync($"{RUTA_PERSONA}/{id}");
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = $"GET {RUTA_PERSONA}/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}.";
                return RedirectToAction(nameof(Index));
            }

            var model = JsonSerializer.Deserialize<Persona>(await resp.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ViewBag.Estados = await CargarEstadosDictAsync(client);   // <-- DICTIONARY
            return View(model);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar([FromForm] Persona persona)
        {
            if (!ModelState.IsValid)
            {
                var client = _http.CreateClient("Api");
                ViewBag.Estados = await CargarEstadosDictAsync(client); // <-- volver a cargar
                return View(persona);
            }

            var http = _http.CreateClient("Api");
            var content = new StringContent(JsonSerializer.Serialize(persona), Encoding.UTF8, "application/json");
            var resp = await http.PutAsync($"{RUTA_PERSONA}/{persona.id_persona}", content);

            if (!resp.IsSuccessStatusCode)
            {
                var client = _http.CreateClient("Api");
                ViewBag.Estados = await CargarEstadosDictAsync(client); // <-- volver a cargar
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"PUT {RUTA_PERSONA}/{persona.id_persona} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
                return View(persona);
            }

            TempData["Ok"] = "Persona actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }


        // ===== Eliminar =====
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _http.CreateClient("Api");

            var resp = await client.GetAsync($"{RUTA_PERSONA}/{id}");
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "La persona no existe o ya fue eliminada.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"GET {RUTA_PERSONA}/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<Persona>(json, JsonOps);

            // Diccionario para mostrar descripción de estado en la vista
            ViewBag.Estados = await CargarEstadosDictAsync(client);

            // Si venís rebotado por conflicto, mostrás motivo
            if (TempData["ApiDetail"] is string det && !string.IsNullOrWhiteSpace(det))
            {
                ViewBag.Bloqueado = true;
                ViewBag.Motivo = det;
            }

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.DeleteAsync($"{RUTA_PERSONA}/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.Conflict || resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = "No se puede eliminar la persona porque está en uso.";
                if (!string.IsNullOrWhiteSpace(body)) TempData["ApiDetail"] = body;
                return RedirectToAction("Eliminar", new { id });
            }

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"DELETE {RUTA_PERSONA}/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
                return RedirectToAction("Eliminar", new { id });
            }

            TempData["Ok"] = "Persona eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ===== Agregar Estado (modal) =====
        [HttpPost]
        public async Task<IActionResult> AgregarEstado([FromBody] Estado_Persona estado)
        {
            if (estado == null || string.IsNullOrWhiteSpace(estado.descripcion))
                return BadRequest("La descripción es obligatoria.");

            var client = _http.CreateClient("Api");
            var content = new StringContent(JsonSerializer.Serialize(estado), Encoding.UTF8, "application/json");

            var resp = await client.PostAsync(RUTA_ESTADO_PERSONA, content);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode, body);

            var creado = JsonSerializer.Deserialize<Estado_Persona>(body, JsonOps)!;
            return Json(new { id = creado.id_estadoPersona, descripcion = creado.descripcion });
        }
    }
}
