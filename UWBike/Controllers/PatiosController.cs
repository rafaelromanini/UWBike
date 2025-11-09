using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UWBike.Connection;
using UWBike.Model;
using UWBike.Common;
using System.ComponentModel.DataAnnotations;
using DTOs;
using UWBike.Interfaces;

namespace UWBike.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PatiosController : ControllerBase
    {
        private readonly IPatioService _patioService;

        public PatiosController(IPatioService patioService)
        {
            _patioService = patioService;
        }

        /// <summary>
        /// Obtém todos os pátios com paginação
        /// </summary>
        /// <param name="parameters">Parâmetros de paginação</param>
        /// <returns>Lista paginada de pátios</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<PatioDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PagedResult<PatioDto>>> GetAll([FromQuery] PaginationParameters parameters)
        {
            try
            {
                var pagedResult = await _patioService.GetAllAsync(parameters);

                // Adicionar links HATEOAS para paginação
                HateoasHelper.AddPaginationLinks(
                    pagedResult,
                    nameof(GetAll),
                    "Patios",
                    Url,
                    new
                    {
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
        /// Obtém um pátio específico por ID
        /// </summary>
        /// <param name="id">ID do pátio</param>
        /// <returns>Dados do pátio</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<PatioDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<PatioDto>>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<PatioDto>.ErrorResponse("ID deve ser maior que zero"));
                }
                var patioDto = await _patioService.GetByIdAsync(id);
                if (patioDto == null)
                {
                    return NotFound(ApiResponse<PatioDto>.ErrorResponse("Pátio não encontrado"));
                }

                var response = ApiResponse<PatioDto>.SuccessResponse(patioDto, "Pátio encontrado com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Patios", id, Url);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PatioDto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtém as motos de um pátio específico
        /// </summary>
        /// <param name="id">ID do pátio</param>
        /// <param name="parameters">Parâmetros de paginação</param>
        /// <returns>Lista paginada de motos do pátio</returns>
        [HttpGet("{id:int}/motos")]
        [ProducesResponseType(typeof(PagedResult<Moto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PagedResult<Moto>>> GetMotos(int id, [FromQuery] PaginationParameters parameters)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("ID deve ser maior que zero"));
                }
                var pagedMotos = await _patioService.GetMotosFromPatioAsync(id, parameters);
                return Ok(pagedMotos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cria um novo pátio
        /// </summary>
        /// <param name="patioDto">Dados do pátio a ser criado</param>
        /// <returns>Pátio criado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatioDto>), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<PatioDto>>> Create([FromBody] CreatePatioDto patioDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<PatioDto>.ErrorResponse("Dados inválidos", errors));
                }
                var created = await _patioService.CreateAsync(patioDto);
                var response = ApiResponse<PatioDto>.SuccessResponse(created, "Pátio criado com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Patios", created.Id, Url);

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PatioDto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Atualiza um pátio existente
        /// </summary>
        /// <param name="id">ID do pátio</param>
        /// <param name="patioDto">Dados atualizados do pátio</param>
        /// <returns>Pátio atualizado</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<PatioDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<PatioDto>>> Update(int id, [FromBody] UpdatePatioDto patioDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<PatioDto>.ErrorResponse("ID deve ser maior que zero"));
                }
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<PatioDto>.ErrorResponse("Dados inválidos", errors));
                }

                var updated = await _patioService.UpdateAsync(id, patioDto);
                var response = ApiResponse<PatioDto>.SuccessResponse(updated, "Pátio atualizado com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Patios", id, Url);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PatioDto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Remove um pátio
        /// </summary>
        /// <param name="id">ID do pátio</param>
        /// <returns>Confirmação de remoção</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("ID deve ser maior que zero"));
                }
                try
                {
                    var deleted = await _patioService.DeleteAsync(id);
                    var response = ApiResponse<object>.SuccessResponse(new object(), "Pátio removido com sucesso");
                    response.Links.Add(new Link(Url.Action(nameof(GetAll), "Patios")!, "list"));
                    return Ok(response);
                }
                catch (InvalidOperationException ex)
                {
                    // Distinguish not found vs business rule (has motos)
                    if (ex.Message.Contains("não encontrado"))
                        return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));

                    return Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }
    }
}