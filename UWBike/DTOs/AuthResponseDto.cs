namespace UWBike.DTOs;

/// <summary>
/// DTO para resposta de autenticação
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Token JWT gerado
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Data de expiração do token
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Email do usuário autenticado
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nome do usuário autenticado
    /// </summary>
    public string Nome { get; set; } = string.Empty;
}
