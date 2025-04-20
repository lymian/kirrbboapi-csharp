using apikirbbo.Models;
using apikirbbo.Repositories;

namespace apikirbbo.Services
{
    public class CategoriaService
    {
        private readonly CategoriaRepository _categoriaRepository;
        public CategoriaService(CategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }
        public IEnumerable<Categoria> ObtenerCategorias()
        {
            return _categoriaRepository.ObtenerCategorias();
        }
        public Categoria? ObtenerCategoriaPorId(int id)
        {
            return _categoriaRepository.ObtenerCategoriaPorId(id);
        }
    }
}