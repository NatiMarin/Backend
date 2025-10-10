using Microsoft.AspNetCore.Mvc;
using SantaRamona.Backoffice.Models;
using System.Text;
using System.Text.Json;

namespace SantaRamona.Backoffice.Controllers
{
    public class EstadoFormularioController : Controller
    {
        private readonly IHttpClientFactory _http;
        private const string ApiPath = "/api/EstadoFormulario";
        private const string ApiFormularios = "/api/formulario";

        public EstadoFormularioController(IHttpClientFactory http) => _http = http;

        // GET: /EstadoFormulario
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync(ApiPath);

            if (!resp.IsSuccessStatusCode)
            {
                ViewBag.ApiError = $"GET {ApiPath} -> {(int)resp.StatusCode} {resp.ReasonPhrase}";
                return View(Enumerable.Empty<Estado_Formulario>());
            }

            var json = await resp.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<IEnumerable<Estado_Formulario>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? Enumerable.Empty<Estado_Formulario>();

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
            if (TempData["Error"] is string err) ViewBag.Error = err;

            return View(data);
        }

        // GET: /EstadoFormulario/Crear
        [HttpGet]
        public IActionResult Crear() => View(new Estado_Formulario());

        // POST: /EstadoFormulario/Crear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([FromForm] string descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                ModelState.AddModelError(nameof(Estado_Formulario.descripcion), "La descripción es obligatoria.");
                return View(new Estado_Formulario { descripcion = descripcion ?? string.Empty });
            }

            var model = new Estado_Formulario { descripcion = descripcion.Trim() };
            var client = _http.CreateClient("Api");

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PostAsync(ApiPath, content);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"POST {ApiPath} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(model);
            }

            // Quedar en pantalla para crear varias
            ViewBag.Ok = "Estado de formulario creado correctamente.";
            ModelState.Clear();
            return View(new Estado_Formulario());
        }

        // GET: /EstadoFormulario/Modificar/5
        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync($"{ApiPath}/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "El estado de formulario no existe.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"GET {ApiPath}/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var model = System.Text.Json.JsonSerializer.Deserialize<Estado_Formulario>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
            return View(model);
        }

        // POST: /EstadoFormulario/Modificar
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar([FromForm] int id_estadoFormulario, [FromForm] string descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                ModelState.AddModelError(nameof(Estado_Formulario.descripcion), "La descripción es obligatoria.");
                return View(new Estado_Formulario { id_estadoFormulario = id_estadoFormulario, descripcion = descripcion ?? string.Empty });
            }

            var model = new Estado_Formulario { id_estadoFormulario = id_estadoFormulario, descripcion = descripcion.Trim() };
            var client = _http.CreateClient("Api");

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PutAsync($"/api/EstadoFormulario/{id_estadoFormulario}", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"PUT {ApiPath}/{id_estadoFormulario} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Respuesta: {body}";
                return View(model);
            }

            TempData["Ok"] = "Estado de formulario actualizado correctamente.";
            return RedirectToAction(nameof(Modificar), new { id = id_estadoFormulario });
        }

        // GET: /EstadoFormulario/Eliminar/5
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _http.CreateClient("Api");

            var r = await client.GetAsync($"{ApiPath}/{id}");
            if (!r.IsSuccessStatusCode)
            {
                TempData["Error"] = r.StatusCode == System.Net.HttpStatusCode.NotFound
                    ? "El estado de formulario no existe o ya fue eliminado."
                    : $"No se pudo obtener el estado (código {(int)r.StatusCode}).";
                return RedirectToAction(nameof(Index));
            }

            var model = await r.Content.ReadFromJsonAsync<Estado_Formulario>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // ¿Está en uso por algún formulario?
            bool enUso = false;
            try
            {
                int pagina = 1;
                const int pageSize = 50;
                while (true)
                {
                    var a = await client.GetAsync($"{ApiFormularios}?pagina={pagina}&pageSize={pageSize}");
                    if (!a.IsSuccessStatusCode) break;

                    var formularios = await a.Content.ReadFromJsonAsync<List<FormularioMin>>();
                    if (formularios == null || formularios.Count == 0) break;

                    if (formularios.Any(x => x.id_estadoFormulario == id))
                    {
                        enUso = true;
                        break;
                    }

                    if (formularios.Count < pageSize) break;
                    pagina++;
                    if (pagina > 2000) break;
                }
            }
            catch { enUso = false; }

            ViewBag.EnUso = enUso;
            return View(model!);
        }

        private class FormularioMin
        {
            public int id_formulario { get; set; }
            public int id_estadoFormulario { get; set; }
        }

        // POST: /EstadoFormulario/Eliminar/5
        [HttpPost, ValidateAntiForgeryToken, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.DeleteAsync($"{ApiPath}/{id}");
            var body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                TempData["Ok"] = "Estado de formulario eliminado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            // Volvemos a cargar el modelo para quedarnos en la misma vista
            var r = await client.GetAsync($"{ApiPath}/{id}");
            if (!r.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudo eliminar el estado de formulario. Intentá nuevamente.";
                return RedirectToAction(nameof(Index));
            }
            var model = await r.Content.ReadFromJsonAsync<Estado_Formulario>(
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
                TempData["Error"] = "No se puede eliminar el estado porque está en uso por uno o más formularios.";
                return View("Eliminar", model!);
            }

            ViewBag.EnUso = false;
            TempData["Error"] = "No se pudo eliminar el estado de formulario. Intentá nuevamente.";
            return View("Eliminar", model!);
        }
    }
}

