using apikirbbo.Models;
using apikirbbo.Repositories;

namespace apikirbbo.Services
{
    public class ClienteService
    {
        private readonly ClienteRepository _clienteRepository;

        public ClienteService(ClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }
        public IEnumerable<Cliente> ObtenerClientes()
        {
            return _clienteRepository.ObtenerClientes();
        }
        public Cliente? ObtenerClientePorId(int id)
        {
            return _clienteRepository.obtenerClientePorId(id);
        }
        public Cliente? ObtenerClientePorIdUsuario(int idUsuario)
        {
            return _clienteRepository.obtenerClientePorIdUsuario(idUsuario);
        }
    }
}
