using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public class CreatePatioDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Endereço é obrigatório")]
        [StringLength(200, ErrorMessage = "Endereço deve ter no máximo 200 caracteres")]
        public string Endereco { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacidade é obrigatória")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacidade deve ser maior que zero")]
        public int Capacidade { get; set; }

        [StringLength(10, ErrorMessage = "CEP deve ter no máximo 10 caracteres")]
        public string? Cep { get; set; }

        [StringLength(50, ErrorMessage = "Cidade deve ter no máximo 50 caracteres")]
        public string? Cidade { get; set; }

        [StringLength(2, ErrorMessage = "Estado deve ter no máximo 2 caracteres")]
        public string? Estado { get; set; }

        [StringLength(15, ErrorMessage = "Telefone deve ter no máximo 15 caracteres")]
        public string? Telefone { get; set; }
    }
}