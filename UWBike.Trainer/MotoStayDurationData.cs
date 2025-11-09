using Microsoft.ML.Data;

namespace UWBike.Trainer;

/// <summary>
/// Dados de entrada para treinamento do modelo de previsão de tempo de permanência
/// </summary>
public class MotoStayDurationData
{
    /// <summary>
    /// Capacidade máxima do pátio
    /// </summary>
    [LoadColumn(0)]
    public float CapacidadePatio { get; set; }

    /// <summary>
    /// Número atual de motos no pátio
    /// </summary>
    [LoadColumn(1)]
    public float MotosNoPatio { get; set; }

    /// <summary>
    /// Taxa de ocupação atual do pátio (%)
    /// </summary>
    [LoadColumn(2)]
    public float TaxaOcupacao { get; set; }

    /// <summary>
    /// Dia da semana (0 = Domingo, 6 = Sábado)
    /// </summary>
    [LoadColumn(3)]
    public float DiaSemana { get; set; }

    /// <summary>
    /// Hora do dia (0-23)
    /// </summary>
    [LoadColumn(4)]
    public float HoraDoDia { get; set; }

    /// <summary>
    /// Média de permanência histórica do pátio (em horas)
    /// </summary>
    [LoadColumn(5)]
    public float MediaPermanenciaHistorica { get; set; }

    /// <summary>
    /// Tempo de permanência real (em horas) - Label para treinamento
    /// </summary>
    [LoadColumn(6)]
    [ColumnName("Label")]
    public float TempoPermanencia { get; set; }
}

/// <summary>
/// Resultado da previsão de tempo de permanência
/// </summary>
public class MotoStayDurationPrediction
{
    /// <summary>
    /// Tempo de permanência previsto (em horas)
    /// </summary>
    [ColumnName("Score")]
    public float TempoPermanenciaPrevisto { get; set; }
}
