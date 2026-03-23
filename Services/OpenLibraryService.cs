using BookRadar.Models;
using System.Text.Json;

namespace BookRadar.Services
{
    public class OpenLibraryService : IOpenLibraryService
    {
        private readonly HttpClient _httpClient;

        public OpenLibraryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<LibroViewModel>> BuscarLibrosAsync(string autor)
        {
            var url = $"https://openlibrary.org/search.json?author={Uri.EscapeDataString(autor)}&limit=20&fields=title,first_publish_year,publisher";


            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var resultado = JsonSerializer.Deserialize<OpenLibraryResponse>(json, options);

            if (resultado?.Docs == null) return new List<LibroViewModel>();

            return resultado.Docs.Select(doc => new LibroViewModel
            {
                Titulo = doc.Title,
                AnioPublicacion = doc.FirstPublishYear,
                Editorial = doc.Publisher?.FirstOrDefault()
            }).ToList();
        }
    }
}