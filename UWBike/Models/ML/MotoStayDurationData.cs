using Microsoft.ML.Data;

namespace UWBike.Models.ML;

/// <summary>
/// Dados de entrada para previsão de tempo de permanência
/// </summary>
public class MotoStayDurationData
{
    public float CapacidadePatio { get; set; }
    public float MotosNoPatio { get; set; }
    public float TaxaOcupacao { get; set; }
    public float DiaSemana { get; set; }
    public float HoraDoDia { get; set; }
    public float MediaPermanenciaHistorica { get; set; }
}

/// <summary>
/// Resultado da previsão de tempo de permanência
/// </summary>
public class MotoStayDurationPrediction
{
    [ColumnName("Score")]
    public float TempoPermanenciaPrevisto { get; set; }
}
