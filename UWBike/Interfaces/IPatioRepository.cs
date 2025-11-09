using UWBike.Model;
using UWBike.Common;

namespace UWBike.Interfaces
{
    public interface IPatioRepository
    {
        Task<IEnumerable<Patio>> GetAllAsync();
        Task<Patio?> GetByIdAsync(int id);
        Task<Patio?> GetByIdWithMotosAsync(int id);
        Task<(IEnumerable<Patio> Patios, int TotalRecords)> GetPagedAsync(PaginationParameters parameters);
        Task<(IEnumerable<Moto> Motos, int TotalRecords)> GetMotosFromPatioAsync(int patioId, PaginationParameters parameters);
        Task<Patio> CreateAsync(Patio patio);
        Task<Patio> UpdateAsync(Patio patio);
        Task<bool> DeleteAsync(int id);
        Task<bool> HasMotosAsync(int id);
    }
}
