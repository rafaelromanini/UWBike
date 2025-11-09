using Microsoft.EntityFrameworkCore;
using UWBike.Connection;
using UWBike.Interfaces;
using UWBike.Model;
using UWBike.Common;

namespace UWBike.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<(IEnumerable<Usuario> Usuarios, int TotalRecords)> GetPagedAsync(PaginationParameters parameters)
        {
            var query = _context.Usuarios.AsQueryable();

            // Filtro de busca
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(u => u.Nome.Contains(parameters.Search) ||
                                       u.Email.Contains(parameters.Search));
            }

            // Ordenação
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                switch (parameters.SortBy.ToLower())
                {
                    case "nome":
                        query = parameters.SortDescending ?
                            query.OrderByDescending(u => u.Nome) :
                            query.OrderBy(u => u.Nome);
                        break;
                    case "email":
                        query = parameters.SortDescending ?
                            query.OrderByDescending(u => u.Email) :
                            query.OrderBy(u => u.Email);
                        break;
                    case "datacriacao":
                        query = parameters.SortDescending ?
                            query.OrderByDescending(u => u.DataCriacao) :
                            query.OrderBy(u => u.DataCriacao);
                        break;
                    default:
                        query = query.OrderBy(u => u.Id);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(u => u.Id);
            }

            var totalRecords = await query.CountAsync();
            var usuarios = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (usuarios, totalRecords);
        }

        public async Task<Usuario> CreateAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario> UpdateAsync(Usuario usuario)
        {
            usuario.DataAtualizacao = DateTime.UtcNow;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return false;

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
        {
            var query = _context.Usuarios.Where(u => u.Email.ToLower() == email.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(u => u.Id != excludeId.Value);
            }
            
            return await query.AnyAsync();
        }
    }
}
