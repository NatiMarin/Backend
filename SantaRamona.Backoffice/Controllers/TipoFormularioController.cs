using Microsoft.AspNetCore.Mvc;
using SantaRamona.Backoffice.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SantaRamona.Backoffice.Controllers
{
    public class TipoFormularioController : Controller
    {
        private readonly IHttpClientFactory _http;
        private const string ApiPath = "/api/TipoFormulario";

        public TipoFormularioController(IHttpClientFactory http)
        {
            _http = http;
        }

        // GET: /TipoFormulario
        public async Task<IActionResult> Index(string? q = null)
        {
            var client = _http.CreateClient("Api");
            try
            {
                var data = await client.GetFromJsonAsync<List<Tipo_Formulario>>(ApiPath)
                           ?? new List<Tipo_Formulario>();

                if (!string.IsNullOrWhiteSpace(q))
                    data = data
                        .Where(x => x.descripcion.Contains(q, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                return View(data);
            }
            catch
            {
                TempData["Error"] = "No se pudo conectar con la API para listar los tipos de formulario.";
                return View(new List<Tipo_Formulario>());
            }
        }

        // GET: /TipoFormulario/Crear
        public IActionResult Crear()
        {
            return View(new Tipo_Formulario());
        }

        // POST: /TipoFormulario/Crear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Tipo_Formulario model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _http.CreateClient("Api");
            var resp = await client.PostAsJsonAsync(ApiPath, model);

            if (!resp.IsSuccessStatusCode)
            {
                ViewBag.ApiError = "No se pudo crear el tipo de formulario.";
                return View(model);
            }

            TempData["Ok"] = "Tipo de formulario creado correctamente.";
            // Mantenemos la pantalla para poder cargar varios seguidos (como en Especie)
            ModelState.Clear();
            return View(new Tipo_Formulario());
        }

        /// GET: /TipoFormulario/Modificar/5
        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync($"{ApiPath}/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "El tipo de formulario no existe.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"GET {ApiPath}/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction(nameof(Index));
            }

            var model = await resp.Content.ReadFromJsonAsync<Tipo_Formulario>(
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // si venís del POST, mostrar OK acá
            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;

            return View(model);
        }

        // POST: /TipoFormulario/Modificar
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar([FromForm] int id_tipoFormulario, [FromForm] string descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                ModelState.AddModelError(nameof(Tipo_Formulario.descripcion), "La descripción es obligatoria.");
                return View(new Tipo_Formulario { id_tipoFormulario = id_tipoFormulario, descripcion = descripcion ?? string.Empty });
            }

            var model = new Tipo_Formulario { id_tipoFormulario = id_tipoFormulario, descripcion = descripcion.Trim() };
            var client = _http.CreateClient("Api");

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(model),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PutAsync($"{ApiPath}/{id_tipoFormulario}", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"PUT {ApiPath}/{id_tipoFormulario} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(model);
            }

            // mensaje y redirección a la misma pantalla
            TempData["Ok"] = "Tipo de formulario actualizado correctamente.";
            return RedirectToAction(nameof(Modificar), new { id = id_tipoFormulario });
        }


        /// GET: /TipoFormulario/Eliminar/5
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _http.CreateClient("Api");

            // 1) Traigo el registro
            var r = await client.GetAsync($"{ApiPath}/{id}");
            if (!r.IsSuccessStatusCode)
            {
                TempData["Error"] = r.StatusCode == System.Net.HttpStatusCode.NotFound
                    ? "El tipo de formulario no existe o ya fue eliminado."
                    : $"No se pudo obtener el tipo de formulario (código {(int)r.StatusCode}).";
                return RedirectToAction(nameof(Index));
            }

            var model = await r.Content.ReadFromJsonAsync<Tipo_Formulario>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // 2) ¿Está en uso? (por formularios o por preguntas)
            bool enUso = false;

            try
            {
                // a) ¿Hay formularios con este tipo?
                var f = await client.GetAsync($"/api/Formulario?tipoId={id}&page=1&pageSize=1");
                if (f.IsSuccessStatusCode && !enUso)
                {
                    // Intento 1: ¿es un array plano?
                    try
                    {
                        var arr = await f.Content.ReadFromJsonAsync<List<object>>();
                        enUso = arr != null && arr.Count > 0;
                    }
                    catch
                    {
                        // Intento 2: ¿es un objeto paginado? (items/data/total)
                        using var doc = JsonDocument.Parse(await f.Content.ReadAsStringAsync());
                        var root = doc.RootElement;

                        if (root.ValueKind == JsonValueKind.Object)
                        {
                            // items: []
                            if (root.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                                enUso = items.GetArrayLength() > 0;

                            // data: []
                            if (!enUso && root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                                enUso = data.GetArrayLength() > 0;

                            // total: 0 / >0
                            if (!enUso && root.TryGetProperty("total", out var total) && total.ValueKind == JsonValueKind.Number)
                                enUso = total.GetInt32() > 0;
                        }
                        else if (root.ValueKind == JsonValueKind.Array)
                        {
                            enUso = root.GetArrayLength() > 0;
                        }
                    }
                }

                // b) ¿Hay preguntas asociadas a este tipo?
                if (!enUso)
                {
                    var p = await client.GetAsync($"/api/Pregunta/por-tipo/{id}");
                    if (p.IsSuccessStatusCode)
                    {
                        var preguntas = await p.Content.ReadFromJsonAsync<List<Pregunta>>(
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        enUso = preguntas != null && preguntas.Count > 0;
                    }
                }
            }
            catch
            {
                enUso = false;
            }

            ViewBag.EnUso = enUso;
            return View(model!);
        }

        // POST: /TipoFormulario/Eliminar/5
        [HttpPost, ValidateAntiForgeryToken, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.DeleteAsync($"{ApiPath}/{id}");
            var body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                TempData["Ok"] = "Tipo de formulario eliminado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            // Volvemos a cargar el modelo para quedarnos en la misma vista
            var r = await client.GetAsync($"{ApiPath}/{id}");
            if (!r.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudo eliminar el tipo de formulario. Intentalo nuevamente.";
                return RedirectToAction(nameof(Index));
            }

            var model = await r.Content.ReadFromJsonAsync<Tipo_Formulario>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            bool esFk = resp.StatusCode == System.Net.HttpStatusCode.Conflict
                        || (int)resp.StatusCode == 422
                        || ((int)resp.StatusCode == 500
                            && (body?.Contains("547") == true
                                || body?.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) == true
                                || body?.Contains("FK__", StringComparison.OrdinalIgnoreCase) == true));

            if (esFk)
            {
                ViewBag.EnUso = true;
                TempData["Error"] = "No se puede eliminar el tipo de formulario porque está en uso.";
                return View("Eliminar", model!);
            }

            ViewBag.EnUso = false;
            TempData["Error"] = "No se pudo eliminar el tipo de formulario. Intentalo nuevamente.";
            return View("Eliminar", model!);
        }
    }
}
