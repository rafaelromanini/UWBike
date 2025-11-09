using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using UWBike.DTOs;
using Xunit;

namespace UWBike.Tests.Integration;

/// <summary>
/// Testes de integração para endpoints públicos (Health Check e validações)
/// </summary>
public class PublicEndpointsTests : IClassFixture<UWBikeWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly UWBikeWebApplicationFactory _factory;

    public PublicEndpointsTests(UWBikeWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new
        {
            email = "invalido@uwbike.com",
            senha = "senhaerrada"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/autenticacao/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Email ou senha inválidos", content);
    }
}
