using UWBike.Connection;
using UWBike.Model;
using UWBike.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using DTOs;
using UWBike.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace UWBike.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize] // Requer autenticação para todos os endpoints
    public class MotosController : ControllerBase
    {
        private readonly IMotoService _motoService;

        public MotosController(IMotoService motoService)
        {
            _motoService = motoService;
        }

        /// <summary>
        /// Obtém todas as motos com paginação
        /// </summary>
        /// <param name="parameters">Parâmetros de paginação</param>
        /// <returns>Lista paginada de motos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<MotoDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PagedResult<MotoDto>>> GetAll([FromQuery] PaginationParameters parameters)
        {
            try
            {
                var pagedResult = await _motoService.GetAllAsync(parameters);
                
                // Adicionar links HATEOAS para paginação
                HateoasHelper.AddPaginationLinks(
                    pagedResult, 
                    nameof(GetAll), 
                    "Motos", 
                    Url, 
                    new { 
                        search = parameters.Search, 
                        sortBy = parameters.SortBy, 
                        sortDescending = parameters.SortDescending 
                    }
                );

                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtém uma moto específica por ID
        /// </summary>
        /// <param name="id">ID da moto</param>
        /// <returns>Dados da moto</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<MotoDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<MotoDto>>> GetById(int id)
        {
            try
            {
                var motoDto = await _motoService.GetByIdAsync(id);
                
                if (motoDto == null)
                {
                    return NotFound(ApiResponse<MotoDto>.ErrorResponse("Moto não encontrada"));
                }

                var response = ApiResponse<MotoDto>.SuccessResponse(motoDto, "Moto encontrada com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Motos", id, Url);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<MotoDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<MotoDto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Busca uma moto por placa
        /// </summary>
        /// <param name="placa">Placa da moto</param>
        /// <returns>Dados da moto</returns>
        [HttpGet("buscar")]
        [ProducesResponseType(typeof(ApiResponse<MotoDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<MotoDto>>> GetByPlaca([FromQuery] string placa)
        {
            try
            {
                var motoDto = await _motoService.GetByPlacaAsync(placa);
                
                if (motoDto == null)
                {
                    return NotFound(ApiResponse<MotoDto>.ErrorResponse("Moto não encontrada"));
                }

                var response = ApiResponse<MotoDto>.SuccessResponse(motoDto, "Moto encontrada com sucesso");
                // Note: Como não temos o ID aqui diretamente no DTO, podemos melhorar isso se necessário
                // HateoasHelper.AddHateoasLinks(response, "Motos", motoDto.Id, Url);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<MotoDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<MotoDto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cria uma nova moto
        /// REGRA: Se a moto já existir no sistema (por placa/chassi) mas não tiver pátio,
        /// ela será alocada ao pátio especificado no cadastro
        /// </summary>
        /// <param name="motoDto">Dados da moto a ser criada</param>
        /// <returns>Moto criada ou atualizada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<MotoDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<MotoDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<MotoDto>>> Create([FromBody] CreateMotoDto motoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<MotoDto>.ErrorResponse("Dados inválidos", errors));
                }

                var motoCriada = await _motoService.CreateAsync(motoDto);

                var response = ApiResponse<MotoDto>.SuccessResponse(motoCriada, "Moto criada/alocada com sucesso");
                // HateoasHelper.AddHateoasLinks(response, "Motos", motoCriada.Id, Url);

                return CreatedAtAction(nameof(GetById), new { id = 0 }, response); // Ajustar ID conforme necessário
            }
            catch (InvalidOperationException ex)
            {
                // Pode ser erro de pátio não encontrado ou conflito
                if (ex.Message.Contains("não encontrado"))
                    return NotFound(ApiResponse<MotoDto>.ErrorResponse(ex.Message));
                
                return Conflict(ApiResponse<MotoDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<MotoDto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Atualiza uma moto existente
        /// </summary>
        /// <param name="id">ID da moto</param>
        /// <param name="motoDto">Dados atualizados da moto</param>
        /// <returns>Moto atualizada</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<MotoDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<MotoDto>>> Update(int id, [FromBody] UpdateMotoDto motoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<MotoDto>.ErrorResponse("Dados inválidos", errors));
                }

                var motoAtualizada = await _motoService.UpdateAsync(id, motoDto);

                var response = ApiResponse<MotoDto>.SuccessResponse(motoAtualizada, "Moto atualizada com sucesso");
                // HateoasHelper.AddHateoasLinks(response, "Motos", id, Url);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<MotoDto>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("não encontrada") || ex.Message.Contains("não encontrado"))
                    return NotFound(ApiResponse<MotoDto>.ErrorResponse(ex.Message));
                
                return Conflict(ApiResponse<MotoDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<MotoDto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Remove uma moto
        /// </summary>
        /// <param name="id">ID da moto</param>
        /// <returns>Confirmação de remoção</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            try
            {
                var deleted = await _motoService.DeleteAsync(id);
                
                var response = ApiResponse<object>.SuccessResponse(new object(), "Moto removida com sucesso");
                response.Links.Add(new Link(Url.Action(nameof(GetAll), "Motos")!, "list"));

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }
    }
}
