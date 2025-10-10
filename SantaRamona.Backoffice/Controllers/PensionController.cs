using Microsoft.AspNetCore.Mvc;
using SantaRamona.Backoffice.Models;
using System.Text;
using System.Text.Json;

namespace SantaRamona.Backoffice.Controllers
{
    public class PensionController : Controller
    {
        private readonly IHttpClientFactory _http;
        public PensionController(IHttpClientFactory http) => _http = http;

        private const string RUTA_PENSION = "/api/pension";
        private const string RUTA_ANIMAL = "/api/animal";   // para detectar uso (FK)

        private static readonly JsonSerializerOptions JsonOps = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // GET: /Pension
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync(RUTA_PENSION);

            if (!resp.IsSuccessStatusCode)
            {
                ViewBag.ApiError = $"GET {RUTA_PENSION} -> {(int)resp.StatusCode} {resp.ReasonPhrase}";
                return View(Enumerable.Empty<Pension>());
            }

            var json = await resp.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<IEnumerable<Pension>>(json, JsonOps) ?? Enumerable.Empty<Pension>();

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;
            if (TempData["Error"] is string err) ViewBag.Error = err;

            return View(data);
        }

        // GET: /Pension/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            // Cargamos el form vacío con defaults (fechaIngreso hoy)
            return View(new Pension { fechaIngreso = DateTime.Today });
        }

        // POST: /Pension/Crear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([FromForm] Pension pension)
        {
            if (!ModelState.IsValid)
                return View(pension);

            var client = _http.CreateClient("Api");

            // Si fechaIngreso no viene seteada, seteamos hoy
            if (pension.fechaIngreso == default)
                pension.fechaIngreso = DateTime.Today;

            var content = new StringContent(JsonSerializer.Serialize(pension), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(RUTA_PENSION, content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"POST {RUTA_PENSION} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
                return View(pension);
            }

            // Me quedo en la misma pantalla para poder cargar otra rápida
            ViewBag.Ok = "Pensión creada correctamente.";
            ModelState.Clear();
            return View(new Pension { fechaIngreso = DateTime.Today });
        }

        // GET: /Pension/Modificar/5
        [HttpGet]
        public async Task<IActionResult> Modificar(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.GetAsync($"{RUTA_PENSION}/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["Error"] = "La pensión no existe.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"GET {RUTA_PENSION}/{id} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<Pension>(json, JsonOps);

            if (TempData["Ok"] is string ok) ViewBag.Ok = ok;

            return View(model);
        }

        // POST: /Pension/Modificar
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modificar([FromForm] Pension pension)
        {
            if (!ModelState.IsValid)
                return View(pension);

            var client = _http.CreateClient("Api");

            var content = new StringContent(JsonSerializer.Serialize(pension), Encoding.UTF8, "application/json");
            var resp = await client.PutAsync($"{RUTA_PENSION}/{pension.id_pension}", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ViewBag.ApiError = $"PUT {RUTA_PENSION}/{pension.id_pension} -> {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}";
                return View(pension);
            }

            TempData["Ok"] = "Pensión actualizada correctamente.";
            return RedirectToAction(nameof(Modificar), new { id = pension.id_pension });
        }

        // GET: /Pension/Eliminar/5
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _http.CreateClient("Api");
            var r = await client.GetAsync($"{RUTA_PENSION}/{id}");
            if (!r.IsSuccessStatusCode)
            {
                TempData["Error"] = r.StatusCode == System.Net.HttpStatusCode.NotFound
                    ? "La pensión no existe o ya fue eliminada."
                    : $"No se pudo obtener la pensión (código {(int)r.StatusCode}).";
                return RedirectToAction(nameof(Index));
            }

            var json = await r.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<Pension>(json, JsonOps);

            // ¿Está en uso por alguno/s animales?
            bool enUso = await EstaEnUsoPorAnimalAsync(client, id);
            ViewBag.EnUso = enUso;

            return View(model!);
        }

        // POST: /Pension/Eliminar/5
        [HttpPost, ValidateAntiForgeryToken, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var client = _http.CreateClient("Api");
            var resp = await client.DeleteAsync($"{RUTA_PENSION}/{id}");
            var body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                TempData["Ok"] = "Pensión eliminada correctamente.";
                return RedirectToAction(nameof(Index));
            }

            // Volvemos a cargar el modelo para mostrar la vista de confirmación con el error
            var r = await client.GetAsync($"{RUTA_PENSION}/{id}");
            if (!r.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudo eliminar la pensión. Inténtalo nuevamente.";
                return RedirectToAction(nameof(Index));
            }
            var model = JsonSerializer.Deserialize<Pension>(
                await r.Content.ReadAsStringAsync(), JsonOps
            );

            bool esFk = resp.StatusCode == System.Net.HttpStatusCode.Conflict
                        || (int)resp.StatusCode == 422
                        || ((int)resp.StatusCode == 500
                            && (body?.Contains("547") == true
                                || body?.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) == true
                                || body?.Contains("FK__", StringComparison.OrdinalIgnoreCase) == true));

            if (esFk)
            {
                ViewBag.EnUso = true;
                TempData["Error"] = "No se puede eliminar la pensión porque está en uso.";
                TempData["ApiDetail"] = body;
                return View("Eliminar", model!);
            }

            ViewBag.EnUso = false;
            TempData["Error"] = "No se pudo eliminar la pensión. Inténtalo nuevamente.";
            TempData["ApiDetail"] = body;
            return View("Eliminar", model!);
        }

        // ========= Helpers =========

        /// <summary>
        /// Recorre animales paginados para ver si alguno referencia la pensión.
        /// Ajustá querystring si tu API usa otra convención de paginado.
        /// </summary>
        private async Task<bool> EstaEnUsoPorAnimalAsync(HttpClient client, int idPension)
        {
            try
            {
                int pagina = 1;
                const int pageSize = 50;

                while (true)
                {
                    var a = await client.GetAsync($"{RUTA_ANIMAL}?pagina={pagina}&pageSize={pageSize}");
                    if (!a.IsSuccessStatusCode) break;

                    var animales = await a.Content.ReadFromJsonAsync<List<AnimalMin>>(JsonOps);
                    if (animales == null || animales.Count == 0) break;

                    if (animales.Any(x => x.id_pension.HasValue && x.id_pension.Value == idPension))
                        return true;

                    if (animales.Count < pageSize) break;
                    pagina++;
                    if (pagina > 2000) break; // corta en caso extremo
                }
            }
            catch
            {
                // si falla, no bloqueamos por esto
            }

            return false;
        }

        private class AnimalMin
        {
            public int id_animal { get; set; }
            public int? id_pension { get; set; }
        }
    }
}
