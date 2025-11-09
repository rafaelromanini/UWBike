using Microsoft.EntityFrameworkCore;
using UWBike.Connection;
using UWBike.Interfaces;
using UWBike.Model;
using UWBike.Common;

namespace UWBike.Repositories
{
    public class PatioRepository : IPatioRepository
    {
        private readonly AppDbContext _context;

        public PatioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Patio>> GetAllAsync()
        {
            return await _context.Patios
                .Include(p => p.Motos)
                .ToListAsync();
        }

        public async Task<Patio?> GetByIdAsync(int id)
        {
            return await _context.Patios
                .Include(p => p.Motos)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Patio?> GetByIdWithMotosAsync(int id)
        {
            return await _context.Patios
                .Include(p => p.Motos)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<(IEnumerable<Patio> Patios, int TotalRecords)> GetPagedAsync(PaginationParameters parameters)
        {
            var query = _context.Patios.Include(p => p.Motos).AsQueryable();

            // Filtro de busca
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(p => p.Nome.Contains(parameters.Search) ||
                                       (p.Cidade != null && p.Cidade.Contains(parameters.Search)) ||
                                       p.Endereco.Contains(parameters.Search));
            }

            // Ordenação
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                switch (parameters.SortBy.ToLower())
                {
                    case "nome":
                        query = parameters.SortDescending ?
                            query.OrderByDescending(p => p.Nome) :
                            query.OrderBy(p => p.Nome);
                        break;
                    case "cidade":
                        query = parameters.SortDescending ?
                            query.OrderByDescending(p => p.Cidade) :
                            query.OrderBy(p => p.Cidade);
                        break;
                    case "capacidade":
                        query = parameters.SortDescending ?
                            query.OrderByDescending(p => p.Capacidade) :
                            query.OrderBy(p => p.Capacidade);
                        break;
                    case "datacriacao":
                        query = parameters.SortDescending ?
                            query.OrderByDescending(p => p.DataCriacao) :
                            query.OrderBy(p => p.DataCriacao);
                        break;
                    default:
                        query = query.OrderBy(p => p.Id);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(p => p.Id);
            }

            var totalRecords = await query.CountAsync();
            var patios = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (patios, totalRecords);
        }

        public async Task<(IEnumerable<Moto> Motos, int TotalRecords)> GetMotosFromPatioAsync(int patioId, PaginationParameters parameters)
        {
            var query = _context.Motos
                .Where(m => m.PatioId == patioId)
                .Include(m => m.Patio)
                .AsQueryable();

            // Filtro de busca
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(m => m.Modelo.Contains(parameters.Search) ||
                                       m.Placa.Contains(parameters.Search) ||
                                       m.Chassi.Contains(parameters.Search));
            }

            var totalRecords = await query.CountAsync();
            var motos = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (motos, totalRecords);
        }

        public async Task<Patio> CreateAsync(Patio patio)
        {
            _context.Patios.Add(patio);
            await _context.SaveChangesAsync();
            return patio;
        }

        public async Task<Patio> UpdateAsync(Patio patio)
        {
            patio.DataAtualizacao = DateTime.UtcNow;
            _context.Patios.Update(patio);
            await _context.SaveChangesAsync();
            return patio;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var patio = await _context.Patios.FindAsync(id);
            if (patio == null)
                return false;

            _context.Patios.Remove(patio);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasMotosAsync(int id)
        {
            return await _context.Motos.AnyAsync(m => m.PatioId == id);
        }
    }
}
