using Microsoft.EntityFrameworkCore;
using UWBike.Connection;
using UWBike.Interfaces;
using UWBike.Model;
using UWBike.Common;

namespace UWBike.Repositories
{
    public class MotoRepository : IMotoRepository
    {
        private readonly AppDbContext _context;

        public MotoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Moto>> GetAllAsync()
        {
            return await _context.Motos
                .Include(m => m.Patio)
                .ToListAsync();
        }

        public async Task<Moto?> GetByIdAsync(int id)
        {
            return await _context.Motos
                .Include(m => m.Patio)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Moto?> GetByPlacaAsync(string placa)
        {
            return await _context.Motos
                .Include(m => m.Patio)
                .FirstOrDefaultAsync(m => m.Placa.ToUpper() == placa.ToUpper());
        }

        public async Task<Moto?> GetByChassiAsync(string chassi)
        {
            return await _context.Motos
                .Include(m => m.Patio)
                .FirstOrDefaultAsync(m => m.Chassi.ToUpper() == chassi.ToUpper());
        }

        public async Task<(IEnumerable<Moto> Motos, int TotalRecords)> GetPagedAsync(PaginationParameters parameters)
        {
            var query = _context.Motos.Include(m => m.Patio).AsQueryable();

            // Filtro de busca
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(m => m.Placa.Contains(parameters.Search) ||
                                       m.Chassi.Contains(parameters.Search));
            }

            // Ordenação
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                switch (parameters.SortBy.ToLower())
                {
                    case "modelo":
                        query = parameters.SortDescending ?
                            query.OrderByDescending(m => m.Modelo) :
                            query.OrderBy(m => m.Modelo);
                        break;
                    case "placa":
                        query = parameters.SortDescending ?
                            query.OrderByDescending(m => m.Placa) :
                            query.OrderBy(m => m.Placa);
                        break;
                    case "anofabricacao":
                        query = parameters.SortDescending ?
                            query.OrderByDescending(m => m.AnoFabricacao) :
                            query.OrderBy(m => m.AnoFabricacao);
                        break;
                    case "datacriacao":
                        query = parameters.SortDescending ?
                            query.OrderByDescending(m => m.DataCriacao) :
                            query.OrderBy(m => m.DataCriacao);
                        break;
                    default:
                        query = query.OrderBy(m => m.Id);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(m => m.Id);
            }

            var totalRecords = await query.CountAsync();
            var motos = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (motos, totalRecords);
        }

        public async Task<Moto> CreateAsync(Moto moto)
        {
            _context.Motos.Add(moto);
            await _context.SaveChangesAsync();
            
            // Carregar o pátio associado
            await _context.Entry(moto).Reference(m => m.Patio).LoadAsync();
            
            return moto;
        }

        public async Task<Moto> UpdateAsync(Moto moto)
        {
            moto.DataAtualizacao = DateTime.UtcNow;
            _context.Motos.Update(moto);
            await _context.SaveChangesAsync();
            
            // Recarregar com o pátio atualizado
            await _context.Entry(moto).Reference(m => m.Patio).LoadAsync();
            
            return moto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null)
                return false;

            _context.Motos.Remove(moto);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByPlacaAsync(string placa, int? excludeId = null)
        {
            var query = _context.Motos.Where(m => m.Placa.ToUpper() == placa.ToUpper());
            
            if (excludeId.HasValue)
            {
                query = query.Where(m => m.Id != excludeId.Value);
            }
            
            return await query.AnyAsync();
        }

        public async Task<bool> ExistsByChassiAsync(string chassi, int? excludeId = null)
        {
            var query = _context.Motos.Where(m => m.Chassi.ToUpper() == chassi.ToUpper());
            
            if (excludeId.HasValue)
            {
                query = query.Where(m => m.Id != excludeId.Value);
            }
            
            return await query.AnyAsync();
        }
    }
}
