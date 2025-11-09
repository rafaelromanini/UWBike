using UWBike.Model;
using UWBike.Common;
using UWBike.Controllers;
using DTOs;

namespace UWBike.Interfaces
{
    public interface IUsuarioService
    {
        Task<PagedResult<UsuarioDto>> GetAllAsync(PaginationParameters parameters);
        Task<UsuarioDto?> GetByIdAsync(int id);
        Task<UsuarioDto?> GetByEmailAsync(string email);
        Task<UsuarioDto> UpdateAsync(int id, UpdateUsuarioDto usuarioDto);
        Task<bool> DeleteAsync(int id);
    }
}
