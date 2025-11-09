using Moq;
using UWBike.Common;
using UWBike.DTOs;
using UWBike.Interfaces;
using UWBike.Model;
using UWBike.Services;
using Xunit;

namespace UWBike.Tests.Unit;

/// <summary>
/// Testes unitários para PatioService
/// </summary>
public class PatioServiceTests
{
    private readonly Mock<IPatioRepository> _mockRepository;
    private readonly PatioService _service;

    public PatioServiceTests()
    {
        _mockRepository = new Mock<IPatioRepository>();
        _service = new PatioService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPatioExists_ReturnsPatioDto()
    {
        // Arrange
        var patioId = 1;
        var patio = new Patio
        {
            Id = patioId,
            Nome = "Pátio Central",
            Endereco = "Rua A, 123",
            Capacidade = 100,
            Cep = "01234-567",
            Cidade = "São Paulo",
            Estado = "SP",
            Telefone = "11999999999"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(patioId))
            .ReturnsAsync(patio);

        // Act
        var result = await _service.GetByIdAsync(patioId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(patioId, result.Id);
        Assert.Equal("Pátio Central", result.Nome);
        Assert.Equal(100, result.Capacidade);
        _mockRepository.Verify(r => r.GetByIdAsync(patioId), Times.Once);
    }

    [Fact]
    public async Task GetByIdOrNameAsync_WithNumericId_ReturnsListWithPatio()
    {
        // Arrange
        var identificador = "1";
        var patio = new Patio
        {
            Id = 1,
            Nome = "Pátio Norte",
            Endereco = "Rua B, 456",
            Capacidade = 50,
            Cep = "12345-678",
            Cidade = "Rio de Janeiro",
            Estado = "RJ",
            Telefone = "21988888888"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(patio);

        // Act
        var result = await _service.GetByIdOrNameAsync(identificador);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Pátio Norte", result[0].Nome);
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }
}
