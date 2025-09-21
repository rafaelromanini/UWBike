using System.ComponentModel.DataAnnotations;

namespace UWBike.Model
{
    public class Patio
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Endereco { get; set; }
        
        [MaxLength(10)]
        public string? Cep { get; set; }
        
        [MaxLength(50)]
        public string? Cidade { get; set; }
        
        [MaxLength(2)]
        public string? Estado { get; set; }
        
        [MaxLength(15)]
        public string? Telefone { get; set; }
        
        public int Capacidade { get; set; }
        
        public bool Ativo { get; set; } = true;
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
        public DateTime? DataAtualizacao { get; set; }

        // Relacionamento: Um patio pode ter várias motos
        public virtual ICollection<Moto> Motos { get; set; } = new List<Moto>();

        // Construtor padrão
        public Patio() 
        {
            Nome = string.Empty;
            Endereco = string.Empty;
        }

        // Construtor com parâmetros essenciais
        public Patio(string nome, string endereco, int capacidade)
        {
            Nome = nome;
            Endereco = endereco;
            Capacidade = capacidade;
            DataCriacao = DateTime.UtcNow;
            Ativo = true;
        }
    }
}