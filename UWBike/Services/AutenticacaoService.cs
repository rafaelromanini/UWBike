using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UWBike.DTOs;
using UWBike.Interfaces;
using UWBike.Model;

namespace UWBike.Services;

/// <summary>
/// Serviço de autenticação com JWT
/// </summary>
public class AutenticacaoService : IAutenticacaoService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;

    public AutenticacaoService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
    }

    /// <summary>
    /// Registra um novo usuário com senha criptografada
    /// </summary>
    public async Task<AuthResponseDto?> RegistrarAsync(RegisterDto registerDto)
    {
        // Verificar se o email já existe
        var usuarioExistente = await _usuarioRepository.GetByEmailAsync(registerDto.Email);
        if (usuarioExistente != null)
        {
            return null; // Email já cadastrado
        }

        // Criar novo usuário com senha hasheada
        var usuario = new Usuario
        {
            Nome = registerDto.Nome,
            Email = registerDto.Email,
            Senha = BCrypt.Net.BCrypt.HashPassword(registerDto.Senha),
            DataCriacao = DateTime.UtcNow
        };

        await _usuarioRepository.CreateAsync(usuario);

        // Gerar token
        var token = GerarToken(usuario.Email, usuario.Nome, usuario.Id);
        var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes");

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Email = usuario.Email,
            Nome = usuario.Nome
        };
    }

    /// <summary>
    /// Autentica um usuário verificando email e senha
    /// </summary>
    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        // Buscar usuário por email
        var usuario = await _usuarioRepository.GetByEmailAsync(loginDto.Email);
        if (usuario == null)
        {
            return null; // Usuário não encontrado
        }

        // Verificar senha
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Senha, usuario.Senha))
        {
            return null; // Senha incorreta
        }

        // Gerar token
        var token = GerarToken(usuario.Email, usuario.Nome, usuario.Id);
        var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes");

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Email = usuario.Email,
            Nome = usuario.Nome
        };
    }

    /// <summary>
    /// Gera um token JWT com claims do usuário
    /// </summary>
    public string GerarToken(string email, string nome, int userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, nome),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationMinutes")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
