using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UWBike.Connection;
using UWBike.Model;
using UWBike.Common;
using System.ComponentModel.DataAnnotations;
using UWBike.Interfaces;
using DTOs;
using Microsoft.AspNetCore.Authorization;

namespace UWBike.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize] // Requer autenticação para todos os endpoints
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        /// <summary>
        /// Obtém todos os usuários com paginação
        /// </summary>
        /// <param name="parameters">Parâmetros de paginação</param>
        /// <returns>Lista paginada de usuários</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<UsuarioDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PagedResult<UsuarioDto>>> GetAll([FromQuery] PaginationParameters parameters)
        {
            try
            {
                var pagedResult = await _usuarioService.GetAllAsync(parameters);
                
                // Adicionar links HATEOAS para paginação
                HateoasHelper.AddPaginationLinks(
                    pagedResult, 
                    nameof(GetAll), 
                    "Usuarios", 
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
        /// Obtém um usuário específico por ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <returns>Dados do usuário</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<UsuarioDto>>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<Usuario>.ErrorResponse("ID deve ser maior que zero"));
                }

                var usuario = await _usuarioService.GetByIdAsync(id);
                
                if (usuario == null)
                {
                    return NotFound(ApiResponse<UsuarioDto>.ErrorResponse("Usuário não encontrado"));
                }

                var response = ApiResponse<UsuarioDto>.SuccessResponse(usuario, "Usuário encontrado com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Usuarios", id, Url);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<UsuarioDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UsuarioDto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtém um usuário por email
        /// </summary>
        /// <param name="email">Email do usuário</param>
        /// <returns>Dados do usuário</returns>
        [HttpGet("buscar")]
        [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<UsuarioDto>>> GetByEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(ApiResponse<UsuarioDto>.ErrorResponse("Email é obrigatório"));
                }

                var usuario = await _usuarioService.GetByEmailAsync(email);
                
                if (usuario == null)
                {
                    return NotFound(ApiResponse<UsuarioDto>.ErrorResponse("Usuário não encontrado"));
                }

                var response = ApiResponse<UsuarioDto>.SuccessResponse(usuario, "Usuário encontrado com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Usuarios", usuario.Id, Url);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<UsuarioDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UsuarioDto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Atualiza um usuário existente
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <param name="usuarioDto">Dados atualizados do usuário</param>
        /// <returns>Usuário atualizado</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<UsuarioDto>>> Update(int id, [FromBody] UpdateUsuarioDto usuarioDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<UsuarioDto>.ErrorResponse("ID deve ser maior que zero"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<UsuarioDto>.ErrorResponse("Dados inválidos", errors));
                }

                var usuario = await _usuarioService.UpdateAsync(id, usuarioDto);

                var response = ApiResponse<UsuarioDto>.SuccessResponse(usuario, "Usuário atualizado com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Usuarios", usuario.Id, Url);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<UsuarioDto>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("não encontrado"))
                    return NotFound(ApiResponse<UsuarioDto>.ErrorResponse(ex.Message));
                
                return Conflict(ApiResponse<UsuarioDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UsuarioDto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Remove um usuário
        /// </summary>
        /// <param name="id">ID do usuário</param>
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
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("ID deve ser maior que zero"));
                }

                await _usuarioService.DeleteAsync(id);

                var response = ApiResponse<object>.SuccessResponse(new object(), "Usuário removido com sucesso");
                response.Links.Add(new Link(Url.Action(nameof(GetAll), "Usuarios")!, "list"));

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

    // DTOs para as operações
    public class CreateUsuarioDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(150, ErrorMessage = "Email deve ter no máximo 150 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 255 caracteres")]
        public string Senha { get; set; } = string.Empty;
    }

    public class UpdateUsuarioDto
    {
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string? Nome { get; set; }

        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(150, ErrorMessage = "Email deve ter no máximo 150 caracteres")]
        public string? Email { get; set; }

        [StringLength(255, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 255 caracteres")]
        public string? Senha { get; set; }
    }
}