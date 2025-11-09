using UWBike.Model;

namespace DTOs
{
    /// <summary>
    /// DTO para resposta de dados de usuário (sem senha)
    /// </summary>
    public class UsuarioDto
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public DateTime? DataAtualizacao { get; set; }

        /// <summary>
        /// Converte entidade Usuario para DTO (sem expor a senha)
        /// </summary>
        public static UsuarioDto FromUsuario(Usuario usuario) => new()
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            DataCriacao = usuario.DataCriacao,
            DataAtualizacao = usuario.DataAtualizacao
        };
    }
}
