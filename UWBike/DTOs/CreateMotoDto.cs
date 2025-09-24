using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public class CreateMotoDto
    {
        [Required(ErrorMessage = "Modelo é obrigatório")]
        [StringLength(100, ErrorMessage = "Modelo deve ter no máximo 100 caracteres")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Placa é obrigatória")]
        [StringLength(10, ErrorMessage = "Placa deve ter no máximo 10 caracteres")]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Chassi é obrigatório")]
        [StringLength(20, ErrorMessage = "Chassi deve ter no máximo 20 caracteres")]
        public string Chassi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pátio é obrigatório")]
        public int PatioId { get; set; }

        [Range(1900, 2100, ErrorMessage = "Ano de fabricação deve estar entre 1900 e 2100")]
        public int? AnoFabricacao { get; set; }

        [StringLength(50, ErrorMessage = "Cor deve ter no máximo 50 caracteres")]
        public string? Cor { get; set; }
    }

}