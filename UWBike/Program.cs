using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using UWBike.Connection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuração avançada do Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UWBike API",
        Version = "v1",
        Description = "API RESTful para gerenciamento de motos, usuários e pátios da UWBike",
        Contact = new OpenApiContact
        {
            Name = "Equipe UWBike",
            Email = "contato@uwbike.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
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
    
    // Configurar ordenação das operações
    c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
});

// DbContext com string de conexão
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UWBike API V1");
        c.RoutePrefix = string.Empty; // Para acessar o Swagger na raiz
        c.DocumentTitle = "UWBike API Documentation";
        c.DefaultModelsExpandDepth(-1); // Colapsar modelos por padrão
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.EnableFilter();
        c.EnableDeepLinking();
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Schema Filter para adicionar exemplos
public class ExampleSchemaFilter : Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiSchema schema, Swashbuckle.AspNetCore.SwaggerGen.SchemaFilterContext context)
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
        else if (context.Type == typeof(UWBike.Controllers.CreatePatioDto))
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
        else if (context.Type == typeof(UWBike.Controllers.CreateMotoDto))
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