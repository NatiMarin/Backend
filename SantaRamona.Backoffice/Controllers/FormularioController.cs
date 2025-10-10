using Microsoft.AspNetCore.Mvc;
using SantaRamona.Backoffice.Models;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;

namespace SantaRamona.Backoffice.Controllers
{
    public class FormularioController : Controller
    {
        private readonly IHttpClientFactory _http;
        private const string ApiForm = "/api/formulario";
        private const string ApiTipos = "/api/TipoFormulario";
        private const string ApiEstados = "/api/EstadoFormulario";
        private const string ApiPersonas = "/api/persona";
        private const string ApiUsuarios = "/api/usuario";
        private const string ApiResp = "/api/Respuesta";

        public FormularioController(IHttpClientFactory http) => _http = http;

        // ========= INDEX =========
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _http.CreateClient("Api");
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Formularios
            var rf = await client.GetAsync(ApiForm);
            if (!rf.IsSuccessStatusCode)
            {
                ViewBag.ApiError = $"GET {ApiForm} -> {(int)rf.StatusCode} {rf.ReasonPhrase}";
                ViewBag.TiposMap = new Dictionary<int, string>();
                ViewBag.EstadosMap = new Dictionary<int, string>();
                ViewBag.PersonasMap = new Dictionary<int, string>();
                ViewBag.UsuariosMap = new Dictionary<int, string>();
                return View(Enumerable.Empty<Formulario>());
            }

            var json = await rf.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<IEnumerable<Formulario>>(json, opts) ?? Enumerable.Empty<Formulario>();

            // Mapas auxiliares
            ViewBag.TiposMap = await CargarTiposMap(client, opts);
            ViewBag.EstadosMap = await CargarEstadosMap(client, opts);

            // Nombres genéricos para persona/usuario (si no hay API, quedan por ID)
            ViewBag.PersonasMap = await CargarPersonasMap(client, opts);
            ViewBag.UsuariosMap = await CargarUsuariosMap(client, opts);

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
            if (TempData["Error"] is string err) ViewBag.Error = err;

            return View(data);
        }

        // ========= CREAR =========
        [HttpGet]
        public async Task<IActionResult> Crear(
            int id_persona = 0,
            int id_tipoFormulario = 0,
            int id_estadoFormulario = 0,
            int? id_usuario = null)
        {
            var client = _http.CreateClient("Api");
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            await CargarCombos(client, opts);

            // fecha por defecto: ahora
            var model = new Formulario
            {
                id_persona = id_persona,
                id_tipoFormulario = id_tipoFormulario,
                id_estadoFormulario = id_estadoFormulario,
                id_usuario = id_usuario,
                fechaAltaFormulario = DateTime.Now
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            [FromForm] int id_persona,
            [FromForm] int id_tipoFormulario,
            [FromForm] DateTime fechaAltaFormulario,
            [FromForm] int? id_usuario,
            [FromForm] int id_estadoFormulario)
        {
            if (id_persona <= 0)
                ModelState.AddModelError(nameof(Formulario.id_persona), "La persona es obligatoria.");
            if (id_tipoFormulario <= 0)
                ModelState.AddModelError(nameof(Formulario.id_tipoFormulario), "El tipo de formulario es obligatorio.");
            if (id_estadoFormulario <= 0)
                ModelState.AddModelError(nameof(Formulario.id_estadoFormulario), "El estado de formulario es obligatorio.");
            if (fechaAltaFormulario == default)
                ModelState.AddModelError(nameof(Formulario.fechaAltaFormulario), "La fecha de alta es obligatoria.");

            var model = new Formulario
            {
                id_persona = id_persona,
                id_tipoFormulario = id_tipoFormulario,
                fechaAltaFormulario = fechaAltaFormulario,
                id_usuario = id_usuario,
                id_estadoFormulario = id_estadoFormulario
            };

            var client = _http.CreateClient("Api");
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Siempre recargo combos
            await CargarCombos(client, opts);

            if (!ModelState.IsValid)
                return View(model);

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(ApiForm, content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"POST {ApiForm} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(model);
            }

            ViewBag.Ok = "Formulario creado correctamente.";
            ModelState.Clear();

            // Mantengo selecciones para cargar varios
            return View(new Formulario
            {
                id_persona = id_persona,
                id_tipoFormulario = id_tipoFormulario,
                id_estadoFormulario = id_estadoFormulario,
                id_usuario = id_usuario,
                fechaAltaFormulario = DateTime.Now
            });
        }

        // ========= MODIFICAR =========
        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            var client = _http.CreateClient("Api");
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var r = await client.GetAsync($"{ApiForm}/{id}");
            if (r.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "El formulario no existe.";
                return RedirectToAction(nameof(Index));
            }
            if (!r.IsSuccessStatusCode)
            {
                var body = await r.Content.ReadAsStringAsync();
                TempData["Error"] = $"GET {ApiForm}/{id} -> {(int)r.StatusCode} {r.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction(nameof(Index));
            }

            var model = await r.Content.ReadFromJsonAsync<Formulario>(opts);

            await CargarCombos(client, opts);

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar(
            [FromForm] int id_formulario,
            [FromForm] int id_persona,
            [FromForm] int id_tipoFormulario,
            [FromForm] DateTime fechaAltaFormulario,
            [FromForm] int? id_usuario,
            [FromForm] int id_estadoFormulario)
        {
            if (id_persona <= 0)
                ModelState.AddModelError(nameof(Formulario.id_persona), "La persona es obligatoria.");
            if (id_tipoFormulario <= 0)
                ModelState.AddModelError(nameof(Formulario.id_tipoFormulario), "El tipo de formulario es obligatorio.");
            if (id_estadoFormulario <= 0)
                ModelState.AddModelError(nameof(Formulario.id_estadoFormulario), "El estado de formulario es obligatorio.");
            if (fechaAltaFormulario == default)
                ModelState.AddModelError(nameof(Formulario.fechaAltaFormulario), "La fecha de alta es obligatoria.");

            var model = new Formulario
            {
                id_formulario = id_formulario,
                id_persona = id_persona,
                id_tipoFormulario = id_tipoFormulario,
                fechaAltaFormulario = fechaAltaFormulario,
                id_usuario = id_usuario,
                id_estadoFormulario = id_estadoFormulario
            };

            var client = _http.CreateClient("Api");
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (!ModelState.IsValid)
            {
                await CargarCombos(client, opts);
                return View(model);
            }

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var resp = await client.PutAsync($"{ApiForm}/{id_formulario}", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"PUT {ApiForm}/{id_formulario} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                await CargarCombos(client, opts);
                return View(model);
            }

            TempData["Ok"] = "Formulario actualizado correctamente.";
            return RedirectToAction(nameof(Modificar), new { id = id_formulario });
        }

        // ========= ELIMINAR =========
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _http.CreateClient("Api");
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var r = await client.GetAsync($"{ApiForm}/{id}");
            if (!r.IsSuccessStatusCode)
            {
                TempData["Error"] = r.StatusCode == System.Net.HttpStatusCode.NotFound
                    ? "El formulario no existe o ya fue eliminado."
                    : $"No se pudo obtener el formulario (código {(int)r.StatusCode}).";
                return RedirectToAction(nameof(Index));
            }

            var model = await r.Content.ReadFromJsonAsync<Formulario>(opts);

            // ¿Está en uso? => tiene respuestas asociadas
            bool enUso = false;
            try
            {
                // Opción 1: endpoint filtrado mínimo
                var r1 = await client.GetAsync($"{ApiResp}?formularioId={id}&page=1&pageSize=1");
                if (r1.IsSuccessStatusCode)
                {
                    var list = await r1.Content.ReadFromJsonAsync<List<RespuestaMin>>(opts) ?? new();
                    enUso = list.Any();
                }
                else
                {
                    // Opción 2: endpoint específico
                    var r2 = await client.GetAsync($"{ApiResp}/por-formulario/{id}");
                    if (r2.IsSuccessStatusCode)
                    {
                        var list = await r2.Content.ReadFromJsonAsync<List<RespuestaMin>>(opts) ?? new();
                        enUso = list.Count > 0;
                    }
                }
            }
            catch { enUso = false; }

            ViewBag.EnUso = enUso;
            return View(model!);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.DeleteAsync($"{ApiForm}/{id}");
            var body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                TempData["Ok"] = "Formulario eliminado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            // Volvemos a cargar el modelo para quedarnos en la misma vista
            var r = await client.GetAsync($"{ApiForm}/{id}");
            Formulario? model = null;
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            if (r.IsSuccessStatusCode)
                model = await r.Content.ReadFromJsonAsync<Formulario>(opts);

            // Detecto error típico de FK/uso
            bool esFk = resp.StatusCode == System.Net.HttpStatusCode.Conflict
                        || (int)resp.StatusCode == 422
                        || ((int)resp.StatusCode == 500
                            && (body?.Contains("547") == true
                                || body?.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) == true
                                || body?.Contains("FK__", StringComparison.OrdinalIgnoreCase) == true));

            if (esFk)
            {
                ViewBag.EnUso = true;
                TempData["Error"] = "No se puede eliminar el formulario porque está en uso (tiene respuestas asociadas).";
                return View("Eliminar", model!);
            }

            ViewBag.EnUso = false;
            TempData["Error"] = "No se pudo eliminar el formulario. Intentalo nuevamente.";
            return View("Eliminar", model!);
        }

        // ========= helpers =========
        private async Task CargarCombos(HttpClient client, JsonSerializerOptions opts)
        {
            var tipos = await client.GetAsync(ApiTipos);
            var estados = await client.GetAsync(ApiEstados);
            var personas = await client.GetAsync(ApiPersonas);
            var usuarios = await client.GetAsync(ApiUsuarios);

            ViewBag.TiposMap = tipos.IsSuccessStatusCode
                ? (await tipos.Content.ReadFromJsonAsync<List<Tipo_Formulario>>(opts) ?? new())
                    .Where(t => t.id_tipoFormulario > 0 && !string.IsNullOrWhiteSpace(t.descripcion))
                    .ToDictionary(t => t.id_tipoFormulario, t => t.descripcion!.Trim())
                : new Dictionary<int, string>();

            ViewBag.EstadosMap = estados.IsSuccessStatusCode
                ? (await estados.Content.ReadFromJsonAsync<List<Estado_Formulario>>(opts) ?? new())
                    .Where(e => e.id_estadoFormulario > 0 && !string.IsNullOrWhiteSpace(e.descripcion))
                    .ToDictionary(e => e.id_estadoFormulario, e => e.descripcion!.Trim())
                : new Dictionary<int, string>();

            ViewBag.PersonasMap = personas.IsSuccessStatusCode
                ? (await personas.Content.ReadFromJsonAsync<List<PersonaMin>>(opts) ?? new())
                    .Where(p => p.id_persona > 0)
                    .ToDictionary(p => p.id_persona, p =>
                        string.IsNullOrWhiteSpace(p.nombre) && string.IsNullOrWhiteSpace(p.apellido)
                            ? $"Persona #{p.id_persona}"
                            : $"{(p.apellido ?? "").Trim()}, {(p.nombre ?? "").Trim()}".Trim(new[] { ' ', ',' }))
                : new Dictionary<int, string>();

            ViewBag.UsuariosMap = usuarios.IsSuccessStatusCode
                ? (await usuarios.Content.ReadFromJsonAsync<List<UsuarioMin>>(opts) ?? new())
                    .Where(u => u.id_usuario > 0)
                    .ToDictionary(u => u.id_usuario, u =>
                        !string.IsNullOrWhiteSpace(u.username) ? u.username!.Trim() : $"Usuario #{u.id_usuario}")
                : new Dictionary<int, string>();
        }

        private static async Task<Dictionary<int, string>> CargarTiposMap(HttpClient client, JsonSerializerOptions opts)
        {
            var r = await client.GetAsync(ApiTipos);
            if (!r.IsSuccessStatusCode) return new();
            var list = await r.Content.ReadFromJsonAsync<List<Tipo_Formulario>>(opts) ?? new();
            return list.Where(x => x.id_tipoFormulario > 0 && !string.IsNullOrWhiteSpace(x.descripcion))
                       .ToDictionary(x => x.id_tipoFormulario, x => x.descripcion!.Trim());
        }

        private static async Task<Dictionary<int, string>> CargarEstadosMap(HttpClient client, JsonSerializerOptions opts)
        {
            var r = await client.GetAsync(ApiEstados);
            if (!r.IsSuccessStatusCode) return new();
            var list = await r.Content.ReadFromJsonAsync<List<Estado_Formulario>>(opts) ?? new();
            return list.Where(x => x.id_estadoFormulario > 0 && !string.IsNullOrWhiteSpace(x.descripcion))
                       .ToDictionary(x => x.id_estadoFormulario, x => x.descripcion!.Trim());
        }

        private static async Task<Dictionary<int, string>> CargarPersonasMap(HttpClient client, JsonSerializerOptions opts)
        {
            var r = await client.GetAsync(ApiPersonas);
            if (!r.IsSuccessStatusCode) return new();
            var list = await r.Content.ReadFromJsonAsync<List<PersonaMin>>(opts) ?? new();
            return list.Where(p => p.id_persona > 0)
                       .ToDictionary(p => p.id_persona, p =>
                          string.IsNullOrWhiteSpace(p.nombre) && string.IsNullOrWhiteSpace(p.apellido)
                            ? $"Persona #{p.id_persona}"
                            : $"{(p.apellido ?? "").Trim()}, {(p.nombre ?? "").Trim()}".Trim(new[] { ' ', ',' }));
        }

        private static async Task<Dictionary<int, string>> CargarUsuariosMap(HttpClient client, JsonSerializerOptions opts)
        {
            var r = await client.GetAsync(ApiUsuarios);
            if (!r.IsSuccessStatusCode) return new();
            var list = await r.Content.ReadFromJsonAsync<List<UsuarioMin>>(opts) ?? new();
            return list.Where(u => u.id_usuario > 0)
                       .ToDictionary(u => u.id_usuario, u =>
                          !string.IsNullOrWhiteSpace(u.username) ? u.username!.Trim() : $"Usuario #{u.id_usuario}");
        }

        // ====== clases mínimas auxiliares ======
        private class RespuestaMin
        {
            public int id_respuesta { get; set; }
            public int id_formulario { get; set; }
        }
        private class PersonaMin
        {
            public int id_persona { get; set; }
            public string? nombre { get; set; }
            public string? apellido { get; set; }
        }
        private class UsuarioMin
        {
            public int id_usuario { get; set; }
            public string? username { get; set; }
            public string? nombre { get; set; }
            public string? apellido { get; set; }
        }
    }
}

