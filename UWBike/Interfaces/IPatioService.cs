using UWBike.Model;
using UWBike.Common;
using DTOs;

namespace UWBike.Interfaces
{
    public interface IPatioService
    {
        Task<PagedResult<PatioDto>> GetAllAsync(PaginationParameters parameters);
        Task<PatioDto?> GetByIdAsync(int id);
        Task<List<PatioDto>> GetByIdOrNameAsync(string identificador);
        Task<PagedResult<MotoDto>> GetMotosFromPatioAsync(int patioId, PaginationParameters parameters);
        Task<PatioDto> CreateAsync(CreatePatioDto patioDto);
        Task<PatioDto> UpdateAsync(int id, UpdatePatioDto patioDto);
        Task<bool> DeleteAsync(int id);
    }
}
