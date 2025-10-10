using Microsoft.AspNetCore.Mvc;
using SantaRamona.Backoffice.Models;
using System.Text;
using System.Text.Json;

namespace SantaRamona.Backoffice.Controllers
{
    public class RespuestaController : Controller
    {
        private readonly IHttpClientFactory _http;
        private const string ApiPath = "/api/Respuesta";
        private const string ApiPreg = "/api/Pregunta";
        private const string ApiForm = "/api/formulario";

        public RespuestaController(IHttpClientFactory http) => _http = http;

        // ========= INDEX: lista TODAS las respuestas =========
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _http.CreateClient("Api");
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Respuestas
            var rResp = await client.GetAsync(ApiPath);
            if (!rResp.IsSuccessStatusCode)
            {
                ViewBag.ApiError = $"GET {ApiPath} -> {(int)rResp.StatusCode} {rResp.ReasonPhrase}";
                ViewBag.PreguntasMap = new Dictionary<int, string>();
                ViewBag.FormulariosMap = new Dictionary<int, string>();
                return View(Enumerable.Empty<Respuesta>());
            }
            var respuestasJson = await rResp.Content.ReadAsStringAsync();
            var respuestas = JsonSerializer.Deserialize<IEnumerable<Respuesta>>(respuestasJson, opts)
                            ?? Enumerable.Empty<Respuesta>();

            // Mapas auxiliares para mostrar descripciones en la vista
            var pregMap = new Dictionary<int, string>();
            var rPreg = await client.GetAsync(ApiPreg);
            if (rPreg.IsSuccessStatusCode)
            {
                var j = await rPreg.Content.ReadAsStringAsync();
                var preguntas = JsonSerializer.Deserialize<IEnumerable<PreguntaMin>>(j, opts) ?? Enumerable.Empty<PreguntaMin>();
                pregMap = preguntas
                    .Where(p => p.id_pregunta > 0 && !string.IsNullOrWhiteSpace(p.pregunta))
                    .GroupBy(p => p.id_pregunta)
                    .ToDictionary(g => g.Key, g => g.First().pregunta!.Trim());
            }

            // Para formulario, por ahora texto simple "Formulario #ID"
            var formMap = new Dictionary<int, string>();
            var rForm = await client.GetAsync(ApiForm);
            if (rForm.IsSuccessStatusCode)
            {
                var j = await rForm.Content.ReadAsStringAsync();
                var forms = JsonSerializer.Deserialize<IEnumerable<FormularioMin>>(j, opts) ?? Enumerable.Empty<FormularioMin>();
                formMap = forms
                    .Where(f => f.id_formulario > 0)
                    .GroupBy(f => f.id_formulario)
                    .ToDictionary(g => g.Key, g => $"Formulario #{g.Key}");
            }

            ViewBag.PreguntasMap = pregMap;
            ViewBag.FormulariosMap = formMap;

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
            if (TempData["Error"] is string err) ViewBag.Error = err;

            return View(respuestas);
        }

        // ========= CREAR =========
        // GET: /Respuesta/Crear
        [HttpGet]
        public async Task<IActionResult> Crear(int id_formulario = 0, int id_pregunta = 0)
        {
            var client = _http.CreateClient("Api");
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Cargar combos
            ViewBag.Formularios = await CargarFormularios(client, opts);
            ViewBag.Preguntas = await CargarPreguntas(client, opts);

            // Preselección opcional
            return View(new Respuesta { id_formulario = id_formulario, id_pregunta = id_pregunta });
        }

        // POST: /Respuesta/Crear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([FromForm] int id_formulario, [FromForm] int id_pregunta, [FromForm] string? respuesta)
        {
            if (id_formulario <= 0)
                ModelState.AddModelError(nameof(Respuesta.id_formulario), "El formulario es obligatorio.");
            if (id_pregunta <= 0)
                ModelState.AddModelError(nameof(Respuesta.id_pregunta), "La pregunta es obligatoria.");
            if (string.IsNullOrWhiteSpace(respuesta))
                ModelState.AddModelError(nameof(Respuesta.respuesta), "La respuesta es obligatoria.");

            var model = new Respuesta
            {
                id_formulario = id_formulario,
                id_pregunta = id_pregunta,
                respuesta = (respuesta ?? "").Trim()
            };

            var client = _http.CreateClient("Api");
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Si hay error de validación, recargo combos y vuelvo
            if (!ModelState.IsValid)
            {
                ViewBag.Formularios = await CargarFormularios(client, opts);
                ViewBag.Preguntas = await CargarPreguntas(client, opts);
                return View(model);
            }

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(ApiPath, content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"POST {ApiPath} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                ViewBag.Formularios = await CargarFormularios(client, opts);
                ViewBag.Preguntas = await CargarPreguntas(client, opts);
                return View(model);
            }

            // Quedar en pantalla para seguir cargando
            ViewBag.Ok = "Respuesta creada correctamente.";
            ModelState.Clear();

            // Mantengo selección de formulario/pregunta para cargar varias
            ViewBag.Formularios = await CargarFormularios(client, opts);
            ViewBag.Preguntas = await CargarPreguntas(client, opts);

            return View(new Respuesta { id_formulario = id_formulario, id_pregunta = id_pregunta });
        }

        // ========= MODIFICAR =========
        // GET: /Respuesta/Modificar/5
        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            var client = _http.CreateClient("Api");
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var resp = await client.GetAsync($"{ApiPath}/{id}");
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "La respuesta no existe.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"GET {ApiPath}/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction(nameof(Index));
            }

            var model = await resp.Content.ReadFromJsonAsync<Respuesta>(opts);

            // Combos
            ViewBag.Formularios = await CargarFormularios(client, opts);
            ViewBag.Preguntas = await CargarPreguntas(client, opts);
            ViewBag.FormularioSeleccionado = model?.id_formulario ?? 0;
            ViewBag.PreguntaSeleccionada = model?.id_pregunta ?? 0;

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;

            return View(model);
        }

        // POST: /Respuesta/Modificar
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar([FromForm] int id_respuesta, [FromForm] int id_formulario, [FromForm] int id_pregunta, [FromForm] string? respuesta)
        {
            if (id_formulario <= 0)
                ModelState.AddModelError(nameof(Respuesta.id_formulario), "El formulario es obligatorio.");
            if (id_pregunta <= 0)
                ModelState.AddModelError(nameof(Respuesta.id_pregunta), "La pregunta es obligatoria.");
            if (string.IsNullOrWhiteSpace(respuesta))
                ModelState.AddModelError(nameof(Respuesta.respuesta), "La respuesta es obligatoria.");

            var model = new Respuesta
            {
                id_respuesta = id_respuesta,
                id_formulario = id_formulario,
                id_pregunta = id_pregunta,
                respuesta = (respuesta ?? "").Trim()
            };

            var client = _http.CreateClient("Api");
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (!ModelState.IsValid)
            {
                ViewBag.Formularios = await CargarFormularios(client, opts);
                ViewBag.Preguntas = await CargarPreguntas(client, opts);
                ViewBag.FormularioSeleccionado = id_formulario;
                ViewBag.PreguntaSeleccionada = id_pregunta;
                return View(model);
            }

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var resp = await client.PutAsync($"{ApiPath}/{id_respuesta}", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"PUT {ApiPath}/{id_respuesta} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";

                ViewBag.Formularios = await CargarFormularios(client, opts);
                ViewBag.Preguntas = await CargarPreguntas(client, opts);
                ViewBag.FormularioSeleccionado = id_formulario;
                ViewBag.PreguntaSeleccionada = id_pregunta;
                return View(model);
            }

            TempData["Ok"] = "Respuesta actualizada correctamente.";
            return RedirectToAction(nameof(Modificar), new { id = id_respuesta });
        }

        // ========= ELIMINAR =========
        // GET: /Respuesta/Eliminar/5
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _http.CreateClient("Api");
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var r = await client.GetAsync($"{ApiPath}/{id}");
            if (!r.IsSuccessStatusCode)
            {
                TempData["Error"] = r.StatusCode == System.Net.HttpStatusCode.NotFound
                    ? "La respuesta no existe o ya fue eliminada."
                    : $"No se pudo obtener la respuesta (código {(int)r.StatusCode}).";
                return RedirectToAction(nameof(Index));
            }

            var model = await r.Content.ReadFromJsonAsync<Respuesta>(opts);

            // Para Respuesta no consideramos 'en uso' (se puede eliminar)
            ViewBag.EnUso = false;

            // Extra: mostrar textos en la vista si querés (por ahora lo dejamos a la vista)
            // Podrías cargar pregunta/ formulario acá y pasarlos por ViewBag si te sirve

            return View(model!);
        }

        // POST: /Respuesta/Eliminar/5
        [HttpPost, ValidateAntiForgeryToken, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.DeleteAsync($"{ApiPath}/{id}");
            var body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                TempData["Ok"] = "Respuesta eliminada correctamente.";
                return RedirectToAction(nameof(Index));
            }

            // Volvemos a cargar el modelo para quedarnos en la misma vista si falla
            var r = await client.GetAsync($"{ApiPath}/{id}");
            Respuesta? model = null;
            if (r.IsSuccessStatusCode)
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                model = await r.Content.ReadFromJsonAsync<Respuesta>(opts);
            }

            // Detecto error por FK si aplica (por si tu API lo valida)
            bool esFk = resp.StatusCode == System.Net.HttpStatusCode.Conflict
                        || (int)resp.StatusCode == 422
                        || ((int)resp.StatusCode == 500
                            && (body?.Contains("547") == true
                                || body?.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) == true
                                || body?.Contains("FK__", StringComparison.OrdinalIgnoreCase) == true));

            if (esFk)
            {
                ViewBag.EnUso = true;
                TempData["Error"] = "No se puede eliminar la respuesta porque está en uso.";
                return View("Eliminar", model!);
            }

            ViewBag.EnUso = false;
            TempData["Error"] = "No se pudo eliminar la respuesta. Intentalo nuevamente.";
            return View("Eliminar", model!);
        }

        // ====== helpers para combos ======
        private static async Task<IEnumerable<FormularioMin>> CargarFormularios(HttpClient client, JsonSerializerOptions opts)
        {
            var r = await client.GetAsync(ApiForm);
            if (!r.IsSuccessStatusCode) return Enumerable.Empty<FormularioMin>();
            var j = await r.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<FormularioMin>>(j, opts) ?? Enumerable.Empty<FormularioMin>();
        }

        private static async Task<IEnumerable<PreguntaMin>> CargarPreguntas(HttpClient client, JsonSerializerOptions opts)
        {
            var r = await client.GetAsync(ApiPreg);
            if (!r.IsSuccessStatusCode) return Enumerable.Empty<PreguntaMin>();
            var j = await r.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<PreguntaMin>>(j, opts) ?? Enumerable.Empty<PreguntaMin>();
        }

        // ====== clases mínimas auxiliares ======
        private class PreguntaMin
        {
            public int id_pregunta { get; set; }
            public string? pregunta { get; set; }
        }

        private class FormularioMin
        {
            public int id_formulario { get; set; }
            public int id_tipoFormulario { get; set; } // por si más adelante querés mostrar algo
        }
    }
}

