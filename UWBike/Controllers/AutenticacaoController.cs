using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UWBike.Common;
using UWBike.DTOs;
using UWBike.Interfaces;

namespace UWBike.Controllers;

/// <summary>
/// Controller responsável pela autenticação de usuários
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class AutenticacaoController : ControllerBase
{
    private readonly IAutenticacaoService _autenticacaoService;

    public AutenticacaoController(IAutenticacaoService autenticacaoService)
    {
        _autenticacaoService = autenticacaoService;
    }

    /// <summary>
    /// Registra um novo usuário no sistema
    /// </summary>
    /// <param name="registerDto">Dados para registro</param>
    /// <returns>Token JWT e informações do usuário</returns>
    /// <response code="201">Usuário registrado com sucesso</response>
    /// <response code="400">Dados inválidos ou email já cadastrado</response>
    [HttpPost("registro")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dados de registro inválidos",
                errors
            ));
        }

        var resultado = await _autenticacaoService.RegistrarAsync(registerDto);

        if (resultado == null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Email já cadastrado no sistema",
                new List<string> { "Um usuário com este email já existe" }
            ));
        }

        var response = new ApiResponse<AuthResponseDto>(resultado, true, "Usuário registrado com sucesso");

        // Adicionar links HATEOAS
        response.Links.Add(new Link("/api/v1/autenticacao/login", "login", "POST"));
        response.Links.Add(new Link("/api/v1/usuarios", "list-usuarios", "GET"));

        return CreatedAtAction(nameof(Registrar), response);
    }

    /// <summary>
    /// Autentica um usuário existente
    /// </summary>
    /// <param name="loginDto">Credenciais de login</param>
    /// <returns>Token JWT e informações do usuário</returns>
    /// <response code="200">Login realizado com sucesso</response>
    /// <response code="401">Credenciais inválidas</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Dados de login inválidos",
                errors
            ));
        }

        var resultado = await _autenticacaoService.LoginAsync(loginDto);

        if (resultado == null)
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse(
                "Email ou senha inválidos",
                new List<string> { "Credenciais incorretas" }
            ));
        }

        var response = new ApiResponse<AuthResponseDto>(resultado, true, "Login realizado com sucesso");

        // Adicionar links HATEOAS
        response.Links.Add(new Link("/api/v1/usuarios", "list-usuarios", "GET"));
        response.Links.Add(new Link("/api/v1/motos", "list-motos", "GET"));
        response.Links.Add(new Link("/api/v2/patios", "list-patios", "GET"));

        return Ok(response);
    }
}
