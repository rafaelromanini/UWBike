using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using UWBike.Connection;
using UWBike.Repositories;
using UWBike.Interfaces;
using UWBike.Services;
using UWBike.Common;
using DTOs;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Versionamento
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader()
    );
});

// Permite o Swagger detectar versões da API
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";         // v1, v2
    options.SubstituteApiVersionInUrl = true;
});

// Configurar Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

// Configurar autenticação JWT
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
    };
});

// Configuração avançada do Swagger usando VersionedApiExplorer
builder.Services.AddSwaggerGen(c =>
{
    // Configurar autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. \r\n\r\n" +
                      "Digite 'Bearer' [espaço] e então seu token.\r\n\r\n" +
                      "Exemplo: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "UWBike API - v1", 
        Version = "v1.0",
        Description = @"API RESTful para gerenciamento de motos, usuários e pátios da UWBike com autenticação JWT.
        
**Versão 1.0:**
- Endpoint GET /api/v1/patios/{id} busca pátios apenas por ID (número inteiro)
- Retorna um único objeto PatioDto"
    });

    c.SwaggerDoc("v2", new OpenApiInfo 
    { 
        Title = "UWBike API - v2", 
        Version = "v2.0",
        Description = @"API RESTful para gerenciamento de motos, usuários e pátios da UWBike com autenticação JWT.
        
**Versão 2.0:**
- Endpoint GET /api/v2/patios/{identificador} busca pátios por ID (número) ou Nome (texto)
- Retorna uma lista de PatioDto (suporta busca por nome com múltiplos resultados)"
    });

    c.DocInclusionPredicate((version, desc) =>
    {
        if (desc.RelativePath != null && desc.RelativePath.Contains("/v"))
        {
            var routeVersion = desc.RelativePath.Split('/')[1];
            return routeVersion.StartsWith(version.Replace(".", ""));
        }
        
        return version == "v1"; // default para v1
    });

    // Incluir comentários XML para documentação
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Configurar exemplos para os schemas
    c.SchemaFilter<ExampleSchemaFilter>();
});

// DbContext com string de conexão
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

// Registrar repositories e services seguindo boas práticas (Scoped para trabalhar com DbContext)
builder.Services.AddScoped<IMotoRepository, MotoRepository>();
builder.Services.AddScoped<IMotoService, MotoService>();

builder.Services.AddScoped<IPatioRepository, PatioRepository>();
builder.Services.AddScoped<IPatioService, PatioService>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddScoped<IAutenticacaoService, AutenticacaoService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("CorsPolicy");

var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"UWBike API {description.ApiVersion}");
            c.RoutePrefix = string.Empty;
        }
    });
}

app.UseHttpsRedirection();

// Autenticação e autorização DEVEM vir nessa ordem
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint de Health Check com detalhes em JSON
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration,
                exception = entry.Value.Exception?.Message,
                data = entry.Value.Data
            })
        }, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        await context.Response.WriteAsync(result);
    }
});

app.Run();

// Schema Filter para adicionar exemplos
public class ExampleSchemaFilter : Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter
{
    public void Apply(OpenApiSchema schema, Swashbuckle.AspNetCore.SwaggerGen.SchemaFilterContext context)
    {
        if (context.Type == typeof(UWBike.Controllers.CreateUsuarioDto))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["nome"] = new Microsoft.OpenApi.Any.OpenApiString("João Silva"),
                ["email"] = new Microsoft.OpenApi.Any.OpenApiString("joao.silva@email.com"),
                ["senha"] = new Microsoft.OpenApi.Any.OpenApiString("senha123")
            };
        }
        else if (context.Type == typeof(CreatePatioDto))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["nome"] = new Microsoft.OpenApi.Any.OpenApiString("Pátio Central"),
                ["endereco"] = new Microsoft.OpenApi.Any.OpenApiString("Rua das Flores, 123"),
                ["capacidade"] = new Microsoft.OpenApi.Any.OpenApiInteger(100),
                ["cep"] = new Microsoft.OpenApi.Any.OpenApiString("01234-567"),
                ["cidade"] = new Microsoft.OpenApi.Any.OpenApiString("São Paulo"),
                ["estado"] = new Microsoft.OpenApi.Any.OpenApiString("SP"),
                ["telefone"] = new Microsoft.OpenApi.Any.OpenApiString("11999999999")
            };
        }
        else if (context.Type == typeof(CreateMotoDto))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["modelo"] = new Microsoft.OpenApi.Any.OpenApiString("Honda CB 600F Hornet"),
                ["placa"] = new Microsoft.OpenApi.Any.OpenApiString("ABC-1234"),
                ["chassi"] = new Microsoft.OpenApi.Any.OpenApiString("9C2JB1310CR000001"),
                ["patioId"] = new Microsoft.OpenApi.Any.OpenApiInteger(1),
                ["anoFabricacao"] = new Microsoft.OpenApi.Any.OpenApiInteger(2022),
                ["cor"] = new Microsoft.OpenApi.Any.OpenApiString("Vermelha")
            };
        }
    }
}