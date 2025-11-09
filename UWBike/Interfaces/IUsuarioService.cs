using UWBike.Model;
using UWBike.Common;
using UWBike.Controllers;

namespace UWBike.Interfaces
{
    public interface IUsuarioService
    {
        Task<PagedResult<Usuario>> GetAllAsync(PaginationParameters parameters);
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario> CreateAsync(CreateUsuarioDto usuarioDto);
        Task<Usuario> UpdateAsync(int id, UpdateUsuarioDto usuarioDto);
        Task<bool> DeleteAsync(int id);
    }
}
