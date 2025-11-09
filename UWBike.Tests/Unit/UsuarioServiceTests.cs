using Moq;
using UWBike.DTOs;
using UWBike.Interfaces;
using UWBike.Model;
using UWBike.Services;
using Xunit;

namespace UWBike.Tests.Unit;

/// <summary>
/// Testes unitários para UsuarioService
/// </summary>
public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _mockRepository;
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _mockRepository = new Mock<IUsuarioRepository>();
        _service = new UsuarioService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsUsuarioDto()
    {
        // Arrange
        var usuarioId = 1;
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "João Silva",
            Email = "joao@email.com",
            Senha = BCrypt.Net.BCrypt.HashPassword("senha123")
        };

        _mockRepository.Setup(r => r.GetByIdAsync(usuarioId))
            .ReturnsAsync(usuario);

        // Act
        var result = await _service.GetByIdAsync(usuarioId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(usuarioId, result.Id);
        Assert.Equal("João Silva", result.Nome);
        Assert.Equal("joao@email.com", result.Email);
        _mockRepository.Verify(r => r.GetByIdAsync(usuarioId), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ReturnsUsuarioDto()
    {
        // Arrange
        var email = "joao@email.com";
        var usuario = new Usuario
        {
            Id = 1,
            Nome = "João Silva",
            Email = email,
            Senha = BCrypt.Net.BCrypt.HashPassword("senha123")
        };

        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(usuario);

        // Act
        var result = await _service.GetByEmailAsync(email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("João Silva", result.Nome);
        Assert.Equal(email, result.Email);
        _mockRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }
}
