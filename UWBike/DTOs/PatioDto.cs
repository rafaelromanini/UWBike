using UWBike.Model;

namespace DTOs
{
    public class PatioDto
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Endereco { get; set; } = string.Empty;

        public string? Cep { get; set; }

        public string? Cidade { get; set; }

        public string? Estado { get; set; }

        public string? Telefone { get; set; }

        public int Capacidade { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public DateTime? DataAtualizacao { get; set; }
        public ICollection<MotoDto> Motos { get; set; } = [];
        
        public static PatioDto fromPatio(Patio patio) => new()
        {
            Id = patio.Id,
            Nome = patio.Nome,
            Endereco = patio.Endereco,
            Cep = patio.Cep,
            Cidade = patio.Cidade,
            Estado = patio.Estado,
            Telefone = patio.Telefone,
            Capacidade = patio.Capacidade,
            Ativo = patio.Ativo,
            DataCriacao = patio.DataCriacao,
            DataAtualizacao = patio.DataAtualizacao,
            Motos = patio.Motos?.Select(MotoDto.fromMoto).ToList() ?? []
        };
    }
}