using apikirbbo.DTOs;
using apikirbbo.Models;
using apikirbbo.Repositories;

namespace apikirbbo.Services
{
    public class ProductoService
    {
        private readonly ProductoRepository _productoRepository;
        private readonly CategoriaRepository _categoriaRepository;
        public ProductoService(ProductoRepository productoRepository, CategoriaRepository categoriaRepository)
        {
            _productoRepository = productoRepository;
            _categoriaRepository = categoriaRepository;
        }
        public IEnumerable<Producto> ObtenerProductos()
        {
            return _productoRepository.ObtenerProductos();
        }
        public Producto? ObtenerProductoPorId(int id)
        {
            return _productoRepository.ObtenerProductoPorId(id);
        }
        public Producto? AgregarProducto(ProductoMergeDTO productoMergeDTO)
        {
            if (_categoriaRepository.ObtenerCategoriaPorId(productoMergeDTO.CategoriaId) == null)
            {
                return null;
            }
            return _productoRepository.AgregarProducto(productoMergeDTO);
        }
        public Producto? ActualizarProducto(ProductoMergeDTO productoMergeDTO,int id)
        {
            if (_categoriaRepository.ObtenerCategoriaPorId(productoMergeDTO.CategoriaId) == null)
            {
                return null;
            }
            return _productoRepository.ActualizarProducto(id, productoMergeDTO);
        }
        public String? EliminarProducto(int id)
        {
            if (_productoRepository.ObtenerProductoPorId(id) == null)
            {
                return null;
            }
            return _productoRepository.EliminarProducto(id);
        }
        public Producto? AlternarEstado(int id)
        {
            if (_productoRepository.ObtenerProductoPorId(id) == null)
            {
                return null;
            }
            return _productoRepository.AlternarEstadoProducto(id);
        }
        public IEnumerable<Producto> ObtenerProductosPorEstado(bool estado)
        {
            return _productoRepository.ObtenerProductosPorEstado(estado);
        }
        public IEnumerable<Producto> ObtenerProductosPorCategoria(int categoriaId)
        {
            return _productoRepository.ObtenerProductosPorCategoria(categoriaId);
        }
        public IEnumerable<Producto> ObtenerHablitidosPorCategoria(int categoriaId)
        {
            return _productoRepository.ObtenerHabilitadosProductosPorCategoria(categoriaId);
        }
    }
}
