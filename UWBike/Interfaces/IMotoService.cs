using UWBike.Model;
using UWBike.Common;
using DTOs;

namespace UWBike.Interfaces
{
    public interface IMotoService
    {
        Task<PagedResult<MotoDto>> GetAllAsync(PaginationParameters parameters);
        Task<MotoDto?> GetByIdAsync(int id);
        Task<MotoDto?> GetByPlacaAsync(string placa);
        Task<MotoDto> CreateAsync(CreateMotoDto motoDto);
        Task<MotoDto> UpdateAsync(int id, UpdateMotoDto motoDto);
        Task<bool> DeleteAsync(int id);
    }
}
