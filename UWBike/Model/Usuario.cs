using System.ComponentModel.DataAnnotations;

namespace UWBike.Model
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }
        
        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Senha { get; set; }
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
        public DateTime? DataAtualizacao { get; set; }

        // Construtor padrão
        public Usuario() 
        {
            Nome = string.Empty;
            Email = string.Empty;
            Senha = string.Empty;
        }

        // Construtor com parâmetros
        public Usuario(string nome, string email, string senha)
        {
            Nome = nome;
            Email = email;
            Senha = senha;
            DataCriacao = DateTime.UtcNow;
        }
    }
}