using BookRadar.Models;

namespace BookRadar.Services
{
    public interface IOpenLibraryService
    {
        Task<List<LibroViewModel>> BuscarLibrosAsync(string autor);
    }
}
