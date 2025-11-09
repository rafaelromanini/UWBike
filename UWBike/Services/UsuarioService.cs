using UWBike.Interfaces;
using UWBike.Model;
using UWBike.Common;
using UWBike.Controllers;

namespace UWBike.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<PagedResult<Usuario>> GetAllAsync(PaginationParameters parameters)
        {
            var (usuarios, totalRecords) = await _usuarioRepository.GetPagedAsync(parameters);
            
            return new PagedResult<Usuario>(usuarios.ToList(), parameters.PageNumber, parameters.PageSize, totalRecords);
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero");

            return await _usuarioRepository.GetByIdAsync(id);
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email é obrigatório");

            return await _usuarioRepository.GetByEmailAsync(email);
        }

        public async Task<Usuario> CreateAsync(CreateUsuarioDto usuarioDto)
        {
            // REGRA DE NEGÓCIO: Verificar se já existe usuário com este email
            var exists = await _usuarioRepository.ExistsByEmailAsync(usuarioDto.Email);
            if (exists)
                throw new InvalidOperationException("Já existe um usuário com este email");

            var usuario = new Usuario(usuarioDto.Nome, usuarioDto.Email, usuarioDto.Senha);
            return await _usuarioRepository.CreateAsync(usuario);
        }

        public async Task<Usuario> UpdateAsync(int id, UpdateUsuarioDto usuarioDto)
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
                usuario.Senha = usuarioDto.Senha;

            return await _usuarioRepository.UpdateAsync(usuario);
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
