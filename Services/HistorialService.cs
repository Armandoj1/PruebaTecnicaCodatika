using BookRadar.Data;
using BookRadar.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BookRadar.Services
{
    public class HistorialService : IHistorialService
    {
        private readonly AppDbContext _context;

        public HistorialService(AppDbContext context)
        {
            _context = context;
        }

        public async Task GuardarAsync(string autor, List<LibroViewModel> libros)
        {
            foreach (var libro in libros)
            {
                var paramAutor = new SqlParameter("@Autor", autor);
                var paramTitulo = new SqlParameter("@Titulo", libro.Titulo);
                var paramAnio = new SqlParameter("@AnioPublicacion", (object?)libro.AnioPublicacion ?? DBNull.Value);
                var paramEditorial = new SqlParameter("@Editorial", (object?)libro.Editorial ?? DBNull.Value);
                var paramInsertado = new SqlParameter
                {
                    ParameterName = "@Insertado",
                    SqlDbType = System.Data.SqlDbType.Bit,
                    Direction = System.Data.ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC SP_InsertarLibro @Autor, @Titulo, @AnioPublicacion, @Editorial, @Insertado OUTPUT",
                    paramAutor, paramTitulo, paramAnio, paramEditorial, paramInsertado
                );
            }
        }

        public async Task<List<HistorialBusqueda>> ObtenerHistorialAsync()
        {
            return await _context.HistorialBusquedas
                .OrderByDescending(h => h.FechaConsulta)
                .ToListAsync();
        }


        public async Task LimpiarHistorialAsync()
        {
            var todos = _context.HistorialBusquedas.ToList();
            _context.HistorialBusquedas.RemoveRange(todos);
            await _context.SaveChangesAsync();
        }
    }
}