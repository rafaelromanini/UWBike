using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UWBike.Common;
using UWBike.DTOs;
using UWBike.Services;

namespace UWBike.Controllers;

/// <summary>
/// Controller para previsões de Machine Learning
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class MLPredictionController : ControllerBase
{
    private readonly MLPredictionService _mlService;
    private readonly ILogger<MLPredictionController> _logger;

    public MLPredictionController(MLPredictionService mlService, ILogger<MLPredictionController> logger)
    {
        _mlService = mlService;
        _logger = logger;
    }

    /// <summary>
    /// Prevê o tempo médio de permanência de motos em um pátio específico
    /// </summary>
    /// <param name="patioId">ID do pátio</param>
    /// <returns>Previsão de tempo de permanência com status e recomendações</returns>
    /// <response code="200">Previsão realizada com sucesso</response>
    /// <response code="404">Pátio não encontrado</response>
    /// <response code="500">Erro ao realizar previsão (modelo não encontrado ou erro interno)</response>
    [HttpGet("tempo-permanencia/patio/{patioId}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse<StayDurationPredictionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<StayDurationPredictionResponseDto>>> PredictStayDuration(int patioId)
    {
        try
        {
            _logger.LogInformation($"Iniciando previsão de tempo de permanência para pátio {patioId}");

            var prediction = await _mlService.PredictStayDurationAsync(patioId);

            var response = ApiResponse<StayDurationPredictionResponseDto>.SuccessResponse(
                prediction,
                "Previsão realizada com sucesso"
            );

            // Adiciona links HATEOAS
            response.Links = new List<Link>
            {
                new Link(
                    Url.Action(nameof(PredictStayDuration), "MLPrediction", new { patioId }, Request.Scheme)!,
                    "self",
                    "GET"
                ),
                new Link(
                    Url.Action("GetById", "Patios", new { id = patioId, version = "1.0" }, Request.Scheme)!,
                    "patio-details",
                    "GET"
                ),
                new Link(
                    Url.Action("GetAll", "Motos", new { patioId, version = "1.0" }, Request.Scheme)!,
                    "motos-no-patio",
                    "GET"
                )
            };

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, $"Pátio {patioId} não encontrado");
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Modelo de ML não encontrado");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "Modelo de Machine Learning não encontrado. Execute o projeto UWBike.Trainer para gerar o modelo."
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar previsão");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "Erro interno ao realizar previsão. Verifique os logs para mais detalhes."
            ));
        }
    }
}
