using UWBike.Interfaces;
using UWBike.Model;
using UWBike.Common;
using UWBike.Controllers;
using DTOs;

namespace UWBike.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<PagedResult<UsuarioDto>> GetAllAsync(PaginationParameters parameters)
        {
            var (usuarios, totalRecords) = await _usuarioRepository.GetPagedAsync(parameters);
            var usuarioDtos = usuarios.Select(UsuarioDto.FromUsuario).ToList();
            
            return new PagedResult<UsuarioDto>(usuarioDtos, parameters.PageNumber, parameters.PageSize, totalRecords);
        }

        public async Task<UsuarioDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero");

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            return usuario == null ? null : UsuarioDto.FromUsuario(usuario);
        }

        public async Task<UsuarioDto?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email é obrigatório");

            var usuario = await _usuarioRepository.GetByEmailAsync(email);
            return usuario == null ? null : UsuarioDto.FromUsuario(usuario);
        }

        public async Task<UsuarioDto> UpdateAsync(int id, UpdateUsuarioDto usuarioDto)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero");

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                throw new InvalidOperationException("Usuário não encontrado");

            // REGRA DE NEGÓCIO: Verificar se o email já está sendo usado por outro usuário
            if (!string.IsNullOrWhiteSpace(usuarioDto.Email) && 
                usuarioDto.Email.ToLower() != usuario.Email.ToLower())
            {
                var exists = await _usuarioRepository.ExistsByEmailAsync(usuarioDto.Email, id);
                if (exists)
                    throw new InvalidOperationException("Já existe outro usuário com este email");
            }

            // Atualizar propriedades
            if (!string.IsNullOrWhiteSpace(usuarioDto.Nome))
                usuario.Nome = usuarioDto.Nome;
            
            if (!string.IsNullOrWhiteSpace(usuarioDto.Email))
                usuario.Email = usuarioDto.Email;
            
            if (!string.IsNullOrWhiteSpace(usuarioDto.Senha))
                usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Senha);

            var updated = await _usuarioRepository.UpdateAsync(usuario);
            return UsuarioDto.FromUsuario(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero");

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                throw new InvalidOperationException("Usuário não encontrado");

            return await _usuarioRepository.DeleteAsync(id);
        }
    }
}
