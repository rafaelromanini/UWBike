using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UWBike.Model
{
    public class Moto
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Modelo { get; set; }
        
        [Required]
        [MaxLength(10)]
        public string Placa { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Chassi { get; set; }
        
        [Range(1900, 2100)]
        public int? AnoFabricacao { get; set; }
        
        [MaxLength(50)]
        public string? Cor { get; set; }
        
        public bool Ativo { get; set; } = true;
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
        public DateTime? DataAtualizacao { get; set; }

        // Relacionamento obrigatório com Patio
        [Required]
        public int PatioId { get; set; }
        
        [ForeignKey("PatioId")]
        public virtual Patio Patio { get; set; } = null!;

        // Construtor padrão
        public Moto() 
        {
            Modelo = string.Empty;
            Placa = string.Empty;
            Chassi = string.Empty;
        }

        // Construtor com parâmetros essenciais
        public Moto(string modelo, string placa, string chassi, int patioId)
        {
            Modelo = modelo;
            Placa = placa;
            Chassi = chassi;
            PatioId = patioId;
            DataCriacao = DateTime.UtcNow;
            Ativo = true;
        }

        // Construtor completo para compatibilidade
        public Moto(int id, string modelo, string placa, string chassi) : this(modelo, placa, chassi, 0)
        {
            Id = id;
        }
    }
}
