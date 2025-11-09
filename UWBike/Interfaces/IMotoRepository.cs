using UWBike.Model;
using UWBike.Common;

namespace UWBike.Interfaces
{
    public interface IMotoRepository
    {
        Task<IEnumerable<Moto>> GetAllAsync();
        Task<Moto?> GetByIdAsync(int id);
        Task<Moto?> GetByPlacaAsync(string placa);
        Task<Moto?> GetByChassiAsync(string chassi);
        Task<(IEnumerable<Moto> Motos, int TotalRecords)> GetPagedAsync(PaginationParameters parameters);
        Task<Moto> CreateAsync(Moto moto);
        Task<Moto> UpdateAsync(Moto moto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByPlacaAsync(string placa, int? excludeId = null);
        Task<bool> ExistsByChassiAsync(string chassi, int? excludeId = null);
    }
}
