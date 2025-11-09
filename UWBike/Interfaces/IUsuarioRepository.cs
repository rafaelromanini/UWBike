using UWBike.Model;
using UWBike.Common;

namespace UWBike.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<(IEnumerable<Usuario> Usuarios, int TotalRecords)> GetPagedAsync(PaginationParameters parameters);
        Task<Usuario> CreateAsync(Usuario usuario);
        Task<Usuario> UpdateAsync(Usuario usuario);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
    }
}
