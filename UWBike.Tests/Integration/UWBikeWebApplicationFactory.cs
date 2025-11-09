using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UWBike.Connection;

namespace UWBike.Tests.Integration;

/// <summary>
/// Factory customizada para testes de integração com banco em memória
/// </summary>
public class UWBikeWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly object _lock = new object();
    private static bool _seeded = false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove todos os registros relacionados ao AppDbContext
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));

            // Adiciona DbContext com banco em memória
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("UWBikeTestDb");
            });
        });
        
        builder.UseEnvironment("Testing");
    }
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        
        // Garante que o seed execute apenas uma vez usando lock
        lock (_lock)
        {
            if (!_seeded)
            {
                using (var scope = host.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                    SeedTestData(db);
                    _seeded = true;
                }
            }
        }
        
        return host;
    }

    private static void SeedTestData(AppDbContext context)
    {
        // Verifica se já existem dados para evitar duplicação
        if (context.Usuarios.Any())
        {
            return; // Dados já existem, não precisa popular novamente
        }

        // Adiciona dados de teste
        var usuario = new UWBike.Model.Usuario
        {
            Id = 1,
            Nome = "Usuário Teste",
            Email = "teste@uwbike.com",
            Senha = BCrypt.Net.BCrypt.HashPassword("senha123")
        };

        var patio = new UWBike.Model.Patio
        {
            Id = 1,
            Nome = "Pátio Teste",
            Endereco = "Rua Teste, 123",
            Capacidade = 100,
            Cep = "12345-678",
            Cidade = "São Paulo",
            Estado = "SP",
            Telefone = "11999999999"
        };

        var moto = new UWBike.Model.Moto
        {
            Id = 1,
            Modelo = "Honda CB 500",
            Placa = "ABC-1234",
            Chassi = "9C2JB1310CR000001",
            PatioId = 1,
            AnoFabricacao = 2022,
            Cor = "Vermelha"
        };

        context.Usuarios.Add(usuario);
        context.Patios.Add(patio);
        context.Motos.Add(moto);
        context.SaveChanges();
    }
}
