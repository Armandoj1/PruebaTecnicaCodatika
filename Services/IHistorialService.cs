using BookRadar.Models;

namespace BookRadar.Services
{
    public interface IHistorialService
    {
        Task GuardarAsync(string autor, List<LibroViewModel> libros);
        Task<List<HistorialBusqueda>> ObtenerHistorialAsync();
        Task LimpiarHistorialAsync();
    }
}
