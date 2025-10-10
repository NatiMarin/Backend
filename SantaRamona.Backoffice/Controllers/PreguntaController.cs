using Microsoft.AspNetCore.Mvc;
using SantaRamona.Backoffice.Models;
using System.Text;
using System.Text.Json;

namespace SantaRamona.Backoffice.Controllers
{
    public class PreguntaController : Controller
    {
        private readonly IHttpClientFactory _http;
        private const string ApiBase = "/api";

        public PreguntaController(IHttpClientFactory http) => _http = http;

        // ========= INDEX (por tipo) =========
        /// ========= INDEX: lista TODAS las preguntas =========
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _http.CreateClient("Api");
            var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Preguntas
            var rPreg = await client.GetAsync($"{ApiBase}/Pregunta");
            if (!rPreg.IsSuccessStatusCode)
            {
                ViewBag.ApiError = $"GET {ApiBase}/Pregunta -> {(int)rPreg.StatusCode} {rPreg.ReasonPhrase}";
                ViewBag.TiposMap = new Dictionary<int, string>();
                return View(Enumerable.Empty<Pregunta>());
            }

            var preguntasJson = await rPreg.Content.ReadAsStringAsync();
            var preguntas = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<Pregunta>>(preguntasJson, opts)
                             ?? Enumerable.Empty<Pregunta>();

            // Tipos para mostrar la descripción
            var tiposMap = new Dictionary<int, string>();
            var rTipos = await client.GetAsync($"{ApiBase}/TipoFormulario");
            if (rTipos.IsSuccessStatusCode)
            {
                var tiposJson = await rTipos.Content.ReadAsStringAsync();
                var tipos = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<Tipo_Formulario>>(tiposJson, opts)
                            ?? Enumerable.Empty<Tipo_Formulario>();
                tiposMap = tipos.ToDictionary(t => t.id_tipoFormulario, t => t.descripcion);
            }

            ViewBag.TiposMap = tiposMap;
            return View(preguntas);
        }


        // ========= CREAR =========
        // GET: /Pregunta/Crear?id_tipoFormulario=1 (id opcional)
        [HttpGet]
        public async Task<IActionResult> Crear(int id_tipoFormulario = 0)
        {
            var client = _http.CreateClient("Api");

            // Tipos para el <select>
            var rTipos = await client.GetAsync($"{ApiBase}/TipoFormulario");
            if (!rTipos.IsSuccessStatusCode)
            {
                ViewBag.ApiError = $"GET {ApiBase}/TipoFormulario -> {(int)rTipos.StatusCode} {rTipos.ReasonPhrase}";
                ViewBag.Tipos = Enumerable.Empty<Tipo_Formulario>();
            }
            else
            {
                var jsonTipos = await rTipos.Content.ReadAsStringAsync();
                ViewBag.Tipos = JsonSerializer.Deserialize<IEnumerable<Tipo_Formulario>>(jsonTipos,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? Enumerable.Empty<Tipo_Formulario>();
            }

            ViewBag.TipoSeleccionado = id_tipoFormulario;
            return View(new Pregunta { id_tipoFormulario = id_tipoFormulario });
        }

        // POST: /Pregunta/Crear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([FromForm] int id_tipoFormulario, [FromForm] string? pregunta)
        {
            if (id_tipoFormulario <= 0)
                ModelState.AddModelError(nameof(Pregunta.id_tipoFormulario), "El tipo de formulario es obligatorio.");
            if (string.IsNullOrWhiteSpace(pregunta))
                ModelState.AddModelError(nameof(Pregunta.pregunta), "La pregunta es obligatoria.");

            var client = _http.CreateClient("Api");
            // Recargo tipos para el select si hay error
            var rTipos = await client.GetAsync($"{ApiBase}/TipoFormulario");
            var tipos = Enumerable.Empty<Tipo_Formulario>();
            if (rTipos.IsSuccessStatusCode)
            {
                var jsonTipos = await rTipos.Content.ReadAsStringAsync();
                tipos = JsonSerializer.Deserialize<IEnumerable<Tipo_Formulario>>(jsonTipos,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? Enumerable.Empty<Tipo_Formulario>();
            }
            ViewBag.Tipos = tipos;
            ViewBag.TipoSeleccionado = id_tipoFormulario;

            var model = new Pregunta { id_tipoFormulario = id_tipoFormulario, pregunta = pregunta?.Trim() ?? string.Empty };
            if (!ModelState.IsValid)
                return View(model);

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync($"{ApiBase}/Pregunta", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"POST {ApiBase}/Pregunta -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(model);
            }

            // Quedo en pantalla para poder cargar varias
            ViewBag.Ok = "Pregunta creada correctamente.";
            ModelState.Clear();
            ViewBag.TipoSeleccionado = id_tipoFormulario;
            return View(new Pregunta { id_tipoFormulario = id_tipoFormulario });
        }

        // ========= MODIFICAR =========
        /// GET: /Pregunta/Modificar/5
        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            var client = _http.CreateClient("Api");
            var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Traer la pregunta
            var resp = await client.GetAsync($"{ApiBase}/Pregunta/{id}");
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "La pregunta no existe.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"GET {ApiBase}/Pregunta/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction(nameof(Index));
            }
            var model = await resp.Content.ReadFromJsonAsync<Pregunta>(opts);

            // Traer tipos para el <select>
            var rTipos = await client.GetAsync($"{ApiBase}/TipoFormulario");
            IEnumerable<Tipo_Formulario> tipos = Enumerable.Empty<Tipo_Formulario>();
            if (rTipos.IsSuccessStatusCode)
            {
                var jsonTipos = await rTipos.Content.ReadAsStringAsync();
                tipos = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<Tipo_Formulario>>(jsonTipos, opts)
                        ?? Enumerable.Empty<Tipo_Formulario>();
            }
            ViewBag.Tipos = tipos;
            ViewBag.TipoSeleccionado = model?.id_tipoFormulario ?? 0;

            // Mostrar OK si venimos del POST
            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;

            return View(model);
        }

        // POST: /Pregunta/Modificar
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar([FromForm] int id_pregunta, [FromForm] int id_tipoFormulario, [FromForm] string? pregunta)
        {
            if (id_tipoFormulario <= 0)
                ModelState.AddModelError(nameof(Pregunta.id_tipoFormulario), "El tipo de formulario es obligatorio.");
            if (string.IsNullOrWhiteSpace(pregunta))
                ModelState.AddModelError(nameof(Pregunta.pregunta), "La pregunta es obligatoria.");

            var model = new Pregunta { id_pregunta = id_pregunta, id_tipoFormulario = id_tipoFormulario, pregunta = (pregunta ?? "").Trim() };

            var client = _http.CreateClient("Api");
            var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Si hay errores, recargar tipos para el select y volver a la vista
            if (!ModelState.IsValid)
            {
                var rTiposErr = await client.GetAsync($"{ApiBase}/TipoFormulario");
                IEnumerable<Tipo_Formulario> tiposErr = Enumerable.Empty<Tipo_Formulario>();
                if (rTiposErr.IsSuccessStatusCode)
                {
                    var jsonTipos = await rTiposErr.Content.ReadAsStringAsync();
                    tiposErr = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<Tipo_Formulario>>(jsonTipos, opts)
                               ?? Enumerable.Empty<Tipo_Formulario>();
                }
                ViewBag.Tipos = tiposErr;
                ViewBag.TipoSeleccionado = id_tipoFormulario;
                return View(model);
            }

            // Persistir
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PutAsync($"{ApiBase}/Pregunta/{id_pregunta}", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();

                // Recargar tipos y mostrar error en la misma vista
                var rTipos = await client.GetAsync($"{ApiBase}/TipoFormulario");
                IEnumerable<Tipo_Formulario> tipos = Enumerable.Empty<Tipo_Formulario>();
                if (rTipos.IsSuccessStatusCode)
                {
                    var jsonTipos = await rTipos.Content.ReadAsStringAsync();
                    tipos = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<Tipo_Formulario>>(jsonTipos, opts)
                            ?? Enumerable.Empty<Tipo_Formulario>();
                }
                ViewBag.Tipos = tipos;
                ViewBag.TipoSeleccionado = id_tipoFormulario;

                ViewBag.ApiError = $"PUT {ApiBase}/Pregunta/{id_pregunta} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(model);
            }

            // Mostrar OK en esta misma pantalla
            TempData["Ok"] = "Pregunta actualizada correctamente.";
            return RedirectToAction(nameof(Modificar), new { id = id_pregunta });
        }


        /// ====== ELIMINAR (pantalla separada) ======
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _http.CreateClient("Api");
            var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Traer la pregunta
            var r = await client.GetAsync($"{ApiBase}/Pregunta/{id}");
            if (!r.IsSuccessStatusCode)
            {
                TempData["Error"] = r.StatusCode == System.Net.HttpStatusCode.NotFound
                    ? "La pregunta no existe o ya fue eliminada."
                    : $"No se pudo obtener la pregunta (código {(int)r.StatusCode}).";
                return RedirectToAction(nameof(Index));
            }
            var model = await r.Content.ReadFromJsonAsync<Pregunta>(opts);

            // === NUEVO: traer descripción del tipo ===
            string tipoDesc = model!.id_tipoFormulario.ToString();
            try
            {
                var rt = await client.GetAsync($"{ApiBase}/TipoFormulario/{model.id_tipoFormulario}");
                if (rt.IsSuccessStatusCode)
                {
                    var tipo = await rt.Content.ReadFromJsonAsync<Tipo_Formulario>(opts);
                    if (!string.IsNullOrWhiteSpace(tipo?.descripcion))
                        tipoDesc = tipo!.descripcion!;
                }
            }
            catch { /* si falla, usamos el fallback con el ID en string */ }
            ViewBag.TipoDesc = tipoDesc;

            // ¿Está en uso? => tiene respuestas
            bool enUso = false;
            try
            {
                var r1 = await client.GetAsync($"{ApiBase}/Respuesta/por-pregunta/{id}");
                if (r1.IsSuccessStatusCode)
                {
                    var list = await r1.Content.ReadFromJsonAsync<List<RespuestaMin>>(opts) ?? new();
                    enUso = list.Count > 0;
                }
                else
                {
                    var r2 = await client.GetAsync($"{ApiBase}/Respuesta?preguntaId={id}&page=1&pageSize=1");
                    if (r2.IsSuccessStatusCode)
                    {
                        var list = await r2.Content.ReadFromJsonAsync<List<RespuestaMin>>(opts) ?? new();
                        enUso = list.Any();
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
            var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var resp = await client.DeleteAsync($"{ApiBase}/Pregunta/{id}");
            var body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                TempData["Ok"] = "Pregunta eliminada correctamente.";
                return RedirectToAction(nameof(Index)); // <- volver al Index general de Pregunta
            }

            // Volvemos a cargar el modelo para quedarnos en la vista
            var r = await client.GetAsync($"{ApiBase}/Pregunta/{id}");
            Pregunta? model = null;
            if (r.IsSuccessStatusCode)
                model = await r.Content.ReadFromJsonAsync<Pregunta>(opts);

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
                TempData["Error"] = "No se puede eliminar la pregunta porque está en uso.";
                return View("Eliminar", model!);
            }

            ViewBag.EnUso = false;
            TempData["Error"] = "No se pudo eliminar la pregunta. Intentalo nuevamente.";
            return View("Eliminar", model!);
        }

        // clase mínima para revisar uso por respuestas
        private class RespuestaMin
        {
            public int id_respuesta { get; set; }
            public int id_pregunta { get; set; }
            public int id_formulario { get; set; }
            public string? respuesta { get; set; }
        }
    }
}

