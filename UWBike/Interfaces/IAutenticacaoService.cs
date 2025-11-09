using UWBike.DTOs;

namespace UWBike.Interfaces;

/// <summary>
/// Interface para o serviço de autenticação
/// </summary>
public interface IAutenticacaoService
{
    /// <summary>
    /// Registra um novo usuário
    /// </summary>
    Task<AuthResponseDto?> RegistrarAsync(RegisterDto registerDto);

    /// <summary>
    /// Autentica um usuário
    /// </summary>
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);

    /// <summary>
    /// Gera um token JWT para um usuário
    /// </summary>
    string GerarToken(string email, string nome, int userId);
}
