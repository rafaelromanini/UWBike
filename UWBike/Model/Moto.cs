using System.ComponentModel.DataAnnotations;

namespace UWBike.Model
{
    public class Moto
    {
        [Key]
        public int Id { get; set; }
        public string Modelo { get; set; }
        public string Placa { get; set; }
        public string Chassi { get; set; }

        // Construtor da classe
        public Moto(int id, string modelo, string placa, string chassi)
        {
            Id = id;
            Modelo = modelo;
            Placa = placa;
            Chassi = chassi;
        }
    }
}
