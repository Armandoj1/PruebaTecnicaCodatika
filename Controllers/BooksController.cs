using BookRadar.Models;
using BookRadar.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookRadar.Controllers
{
    public class BooksController : Controller
    {
        private readonly IOpenLibraryService _libraryService;
        private readonly IHistorialService _historialService;

        public BooksController(IOpenLibraryService libraryService, IHistorialService historialService)
        {
            _libraryService = libraryService;
            _historialService = historialService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> History()
        {
            var historial = await _historialService.ObtenerHistorialAsync();
            return View(historial);
        }

        [HttpPost]
        public async Task<IActionResult> ClearHistory()
        {
            await _historialService.LimpiarHistorialAsync();
            return RedirectToAction("History");
        }

        [HttpPost]
        public async Task<IActionResult> Search(BusquedaResultadoViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View("Index", modelo);

            var libros = await _libraryService.BuscarLibrosAsync(modelo.Autor);
            await _historialService.GuardarAsync(modelo.Autor, libros);

            modelo.Libros = libros;
            return View("Results", modelo);
        }

        public async Task<IActionResult> ExportCsv()
        {
            var historial = await _historialService.ObtenerHistorialAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Autor,Titulo,AnioPublicacion,Editorial,FechaConsulta");

            foreach (var item in historial)
            {
                sb.AppendLine(
                    $"\"{item.Autor}\"," +
                    $"\"{item.Titulo}\"," +
                    $"{item.AnioPublicacion?.ToString() ?? ""}," +
                    $"\"{item.Editorial ?? ""}\"," +
                    $"{item.FechaConsulta:dd/MM/yyyy HH:mm}"
                );
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"historial_bookradar_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}