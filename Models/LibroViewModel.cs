using System.ComponentModel.DataAnnotations;

namespace BookRadar.Models
{
    public class LibroViewModel
    {
        public string Titulo { get; set; } = string.Empty;
        public int? AnioPublicacion { get; set; }
        public string? Editorial { get; set; }
    }

    public class BusquedaResultadoViewModel
    {
        [Required(ErrorMessage = "El nombre del autor es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre del autor no puede superar los 100 caracteres.")]
        [MinLength(2, ErrorMessage = "Ingresa al menos 2 caracteres.")]
        public string Autor { get; set; } = string.Empty;
        public List<LibroViewModel> Libros { get; set; } = new();
    }
}