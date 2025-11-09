namespace UWBike.DTOs;

/// <summary>
/// Resposta da previsão de tempo de permanência
/// </summary>
public class StayDurationPredictionResponseDto
{
    /// <summary>
    /// ID do pátio
    /// </summary>
    public int PatioId { get; set; }

    /// <summary>
    /// Nome do pátio
    /// </summary>
    public string NomePatio { get; set; } = string.Empty;

    /// <summary>
    /// Capacidade máxima do pátio
    /// </summary>
    public int Capacidade { get; set; }

    /// <summary>
    /// Número de motos atualmente no pátio
    /// </summary>
    public int MotosAtuais { get; set; }

    /// <summary>
    /// Taxa de ocupação atual (%)
    /// </summary>
    public float TaxaOcupacao { get; set; }

    /// <summary>
    /// Tempo médio de permanência previsto (em horas)
    /// </summary>
    public float TempoPermanenciaPrevisto { get; set; }

    /// <summary>
    /// Tempo de permanência em formato legível (dias e horas)
    /// </summary>
    public string TempoFormatado { get; set; } = string.Empty;

    /// <summary>
    /// Status da previsão (Muito Rápido, Rápido, Normal, Lento, Muito Lento)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Recomendação baseada na previsão
    /// </summary>
    public string Recomendacao { get; set; } = string.Empty;

    /// <summary>
    /// Data/hora da previsão
    /// </summary>
    public DateTime DataPrevisao { get; set; } = DateTime.UtcNow;
}
