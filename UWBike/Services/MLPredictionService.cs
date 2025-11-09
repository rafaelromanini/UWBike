using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using UWBike.Connection;
using UWBike.DTOs;
using UWBike.Models.ML;

namespace UWBike.Services;

/// <summary>
/// Serviço de Machine Learning para previsões relacionadas ao UWBike
/// </summary>
public class MLPredictionService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MLPredictionService> _logger;
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private const string ModelPath = "uwbike-ml-model.zip";

    public MLPredictionService(IServiceScopeFactory scopeFactory, ILogger<MLPredictionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _mlContext = new MLContext(seed: 0);
        LoadModel();
    }

    /// <summary>
    /// Carrega o modelo de ML pré-treinado do arquivo .zip
    /// </summary>
    private void LoadModel()
    {
        try
        {
            if (!File.Exists(ModelPath))
            {
                _logger.LogError($"Arquivo de modelo não encontrado: {ModelPath}");
                _logger.LogWarning("Execute o projeto UWBike.Trainer para gerar o modelo");
                throw new FileNotFoundException($"Modelo ML não encontrado em {ModelPath}. Execute 'dotnet run --project UWBike.Trainer' para gerar o modelo.");
            }

            // Carrega o modelo do arquivo .zip
            _model = _mlContext.Model.Load(ModelPath, out var modelInputSchema);
            
            _logger.LogInformation($"Modelo de ML carregado com sucesso de {ModelPath}");
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar modelo de ML");
            throw new InvalidOperationException("Falha ao carregar o modelo de ML", ex);
        }
    }

    /// <summary>
    /// Prevê o tempo médio de permanência de motos em um pátio
    /// </summary>
    public async Task<StayDurationPredictionResponseDto> PredictStayDurationAsync(int patioId)
    {
        if (_model == null)
        {
            throw new InvalidOperationException("Modelo de ML não foi carregado");
        }

        // Cria um scope para acessar o DbContext
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Busca informações do pátio
        var patio = await context.Patios.FirstOrDefaultAsync(p => p.Id == patioId);
        if (patio == null)
        {
            throw new KeyNotFoundException($"Pátio com ID {patioId} não encontrado");
        }

        // Conta motos atuais no pátio
        var motosAtuais = await context.Motos
            .CountAsync(m => m.PatioId == patioId);

        // Calcula taxa de ocupação
        var taxaOcupacao = patio.Capacidade > 0 
            ? (float)motosAtuais / patio.Capacidade * 100 
            : 0f;

        // Calcula média de permanência histórica (simulado - em produção, calcular do histórico real)
        // Aqui você poderia ter uma tabela de histórico de entradas/saídas
        var mediaPermanenciaHistorica = await CalcularMediaPermanenciaHistoricaAsync(context, patioId);

        // Dados atuais
        var agora = DateTime.Now;
        var diaSemana = (int)agora.DayOfWeek;
        var horaDoDia = agora.Hour;

        // Prepara dados para previsão
        var input = new MotoStayDurationData
        {
            CapacidadePatio = patio.Capacidade,
            MotosNoPatio = motosAtuais,
            TaxaOcupacao = taxaOcupacao,
            DiaSemana = diaSemana,
            HoraDoDia = horaDoDia,
            MediaPermanenciaHistorica = mediaPermanenciaHistorica
        };

        // Cria o prediction engine
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<MotoStayDurationData, MotoStayDurationPrediction>(_model);
        
        // Faz a previsão
        var prediction = predictionEngine.Predict(input);

        // Garante que o tempo seja positivo
        var tempoPrevisto = Math.Max(0, prediction.TempoPermanenciaPrevisto);

        // Formata o tempo
        var dias = (int)(tempoPrevisto / 24);
        var horas = (int)(tempoPrevisto % 24);
        var tempoFormatado = dias > 0 
            ? $"{dias} dia(s) e {horas} hora(s)" 
            : $"{horas} hora(s)";

        // Determina status e recomendação
        var (status, recomendacao) = GetStatusAndRecommendation(tempoPrevisto);

        return new StayDurationPredictionResponseDto
        {
            PatioId = (int)patio.Id,
            NomePatio = patio.Nome,
            Capacidade = patio.Capacidade,
            MotosAtuais = motosAtuais,
            TaxaOcupacao = taxaOcupacao,
            TempoPermanenciaPrevisto = tempoPrevisto,
            TempoFormatado = tempoFormatado,
            Status = status,
            Recomendacao = recomendacao
        };
    }

    /// <summary>
    /// Calcula a média de permanência histórica
    /// Em produção, isso deveria vir de uma tabela de histórico
    /// </summary>
    private async Task<float> CalcularMediaPermanenciaHistoricaAsync(AppDbContext context, int patioId)
    {
        // Simulação: baseada na capacidade do pátio
        // Em produção real, você teria algo como:
        // var historico = await context.HistoricoMotos
        //     .Where(h => h.PatioId == patioId && h.DataSaida.HasValue)
        //     .Select(h => (h.DataSaida.Value - h.DataEntrada).TotalHours)
        //     .ToListAsync();
        // return historico.Any() ? (float)historico.Average() : 48f;
        
        var patio = await context.Patios.FirstOrDefaultAsync(p => p.Id == patioId);
        if (patio == null) return 48f;

        // Simulação: pátios maiores tendem a ter permanência média menor
        return patio.Capacidade switch
        {
            <= 50 => 55f,
            <= 100 => 48f,
            <= 150 => 45f,
            _ => 42f
        };
    }

    /// <summary>
    /// Determina o status e a recomendação baseado no tempo previsto
    /// </summary>
    private (string Status, string Recomendacao) GetStatusAndRecommendation(float tempoPermanencia)
    {
        return tempoPermanencia switch
        {
            >= 60 => ("Muito Lento", "Tempo de permanência muito alto. Verifique processos de saída e considere otimizações."),
            >= 48 => ("Lento", "Tempo de permanência acima da média. Monitore a rotatividade do pátio."),
            >= 36 => ("Normal", "Tempo de permanência dentro da média esperada."),
            >= 24 => ("Rápido", "Rotatividade acima da média. Bom fluxo de motos."),
            _ => ("Muito Rápido", "Rotatividade excelente. Pátio com alta eficiência.")
        };
    }
}
