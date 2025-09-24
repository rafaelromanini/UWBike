using UWBike.Model;

namespace DTOs
{
    public class MotoDto
    {
        public int Id { get; set; }

        public string Modelo { get; set; } = string.Empty;

        public string Placa { get; set; } = string.Empty;

        public string Chassi { get; set; } = string.Empty;

        public int? AnoFabricacao { get; set; }

        public string? Cor { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public DateTime? DataAtualizacao { get; set; }

        public int PatioId { get; set; }


        public static MotoDto fromMoto(Moto moto) => new()
        {
            Id = moto.Id,
            Modelo = moto.Modelo,
            Placa = moto.Placa,
            Chassi = moto.Chassi,
            AnoFabricacao = moto.AnoFabricacao,
            Cor = moto.Cor,
            Ativo = moto.Ativo,
            DataCriacao = moto.DataCriacao,
            DataAtualizacao = moto.DataAtualizacao,
            PatioId = moto.PatioId
        };
    }
}