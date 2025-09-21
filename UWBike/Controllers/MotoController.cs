using UWBike.Connection;
using UWBike.Model;
using UWBike.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace UWBike.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MotosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MotosController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todas as motos com paginação
        /// </summary>
        /// <param name="parameters">Parâmetros de paginação</param>
        /// <returns>Lista paginada de motos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<Moto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PagedResult<Moto>>> GetAll([FromQuery] PaginationParameters parameters)
        {
            try
            {
                var query = _context.Motos.Include(m => m.Patio).AsQueryable();

                // Filtro de busca por modelo, placa, chassi ou pátio
                if (!string.IsNullOrWhiteSpace(parameters.Search))
                {
                    query = query.Where(m => m.Modelo.Contains(parameters.Search) || 
                                           m.Placa.Contains(parameters.Search) ||
                                           m.Chassi.Contains(parameters.Search) ||
                                           m.Patio.Nome.Contains(parameters.Search));
                }

                // Ordenação
                if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                {
                    switch (parameters.SortBy.ToLower())
                    {
                        case "modelo":
                            query = parameters.SortDescending ? 
                                query.OrderByDescending(m => m.Modelo) : 
                                query.OrderBy(m => m.Modelo);
                            break;
                        case "placa":
                            query = parameters.SortDescending ? 
                                query.OrderByDescending(m => m.Placa) : 
                                query.OrderBy(m => m.Placa);
                            break;
                        case "anofabricacao":
                            query = parameters.SortDescending ? 
                                query.OrderByDescending(m => m.AnoFabricacao) : 
                                query.OrderBy(m => m.AnoFabricacao);
                            break;
                        case "datacriacao":
                            query = parameters.SortDescending ? 
                                query.OrderByDescending(m => m.DataCriacao) : 
                                query.OrderBy(m => m.DataCriacao);
                            break;
                        default:
                            query = query.OrderBy(m => m.Id);
                            break;
                    }
                }
                else
                {
                    query = query.OrderBy(m => m.Id);
                }

                var totalRecords = await query.CountAsync();
                var motos = await query
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync();

                var pagedResult = new PagedResult<Moto>(motos, parameters.PageNumber, parameters.PageSize, totalRecords);
                
                // Adicionar links HATEOAS para paginação
                HateoasHelper.AddPaginationLinks(
                    pagedResult, 
                    nameof(GetAll), 
                    "Motos", 
                    Url, 
                    new { 
                        search = parameters.Search, 
                        sortBy = parameters.SortBy, 
                        sortDescending = parameters.SortDescending 
                    }
                );

                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtém uma moto específica por ID
        /// </summary>
        /// <param name="id">ID da moto</param>
        /// <returns>Dados da moto</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Moto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<Moto>>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<Moto>.ErrorResponse("ID deve ser maior que zero"));
                }

                var moto = await _context.Motos
                    .Include(m => m.Patio)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (moto == null)
                {
                    return NotFound(ApiResponse<Moto>.ErrorResponse("Moto não encontrada"));
                }

                var response = ApiResponse<Moto>.SuccessResponse(moto, "Moto encontrada com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Motos", id, Url);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Moto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Busca uma moto por placa
        /// </summary>
        /// <param name="placa">Placa da moto</param>
        /// <returns>Dados da moto</returns>
        [HttpGet("buscar")]
        [ProducesResponseType(typeof(ApiResponse<Moto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<Moto>>> GetByPlaca([FromQuery] string placa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(placa))
                {
                    return BadRequest(ApiResponse<Moto>.ErrorResponse("Placa é obrigatória"));
                }

                var moto = await _context.Motos
                    .Include(m => m.Patio)
                    .FirstOrDefaultAsync(m => m.Placa.ToUpper() == placa.ToUpper());
                
                if (moto == null)
                {
                    return NotFound(ApiResponse<Moto>.ErrorResponse("Moto não encontrada"));
                }

                var response = ApiResponse<Moto>.SuccessResponse(moto, "Moto encontrada com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Motos", moto.Id, Url);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Moto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cria uma nova moto
        /// REGRA: Se a moto já existir no sistema (por placa/chassi) mas não tiver pátio,
        /// ela será alocada ao pátio especificado no cadastro
        /// </summary>
        /// <param name="motoDto">Dados da moto a ser criada</param>
        /// <returns>Moto criada ou atualizada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Moto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<Moto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<Moto>>> Create([FromBody] CreateMotoDto motoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<Moto>.ErrorResponse("Dados inválidos", errors));
                }

                // Verificar se o pátio existe
                var patio = await _context.Patios.FindAsync(motoDto.PatioId);
                if (patio == null)
                {
                    return NotFound(ApiResponse<Moto>.ErrorResponse("Pátio especificado não encontrado"));
                }

                // REGRA DE NEGÓCIO: Verificar se já existe moto com a mesma placa ou chassi
                var motoExistentePlaca = await _context.Motos
                    .FirstOrDefaultAsync(m => m.Placa.ToUpper() == motoDto.Placa.ToUpper());
                
                var motoExistenteChassi = await _context.Motos
                    .FirstOrDefaultAsync(m => m.Chassi.ToUpper() == motoDto.Chassi.ToUpper());

                // Se encontrou moto com placa existente
                if (motoExistentePlaca != null)
                {
                    // Se a moto já tem um pátio, não pode cadastrar novamente
                    if (motoExistentePlaca.PatioId > 0)
                    {
                        return Conflict(ApiResponse<Moto>.ErrorResponse($"Já existe uma moto com a placa {motoDto.Placa} alocada no pátio {motoExistentePlaca.Patio?.Nome}"));
                    }
                    
                    // Se não tem pátio (PatioId = 0), aloca ao pátio especificado
                    motoExistentePlaca.PatioId = motoDto.PatioId;
                    motoExistentePlaca.DataAtualizacao = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                    
                    // Recarregar com o pátio
                    await _context.Entry(motoExistentePlaca).Reference(m => m.Patio).LoadAsync();
                    
                    var responseUpdate = ApiResponse<Moto>.SuccessResponse(motoExistentePlaca, "Moto existente alocada ao pátio com sucesso");
                    HateoasHelper.AddHateoasLinks(responseUpdate, "Motos", motoExistentePlaca.Id, Url);
                    
                    return Ok(responseUpdate);
                }

                // Se encontrou moto com chassi existente (mesma lógica)
                if (motoExistenteChassi != null)
                {
                    if (motoExistenteChassi.PatioId > 0)
                    {
                        return Conflict(ApiResponse<Moto>.ErrorResponse($"Já existe uma moto com o chassi {motoDto.Chassi} alocada no pátio {motoExistenteChassi.Patio?.Nome}"));
                    }
                    
                    motoExistenteChassi.PatioId = motoDto.PatioId;
                    motoExistenteChassi.DataAtualizacao = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                    
                    await _context.Entry(motoExistenteChassi).Reference(m => m.Patio).LoadAsync();
                    
                    var responseUpdate = ApiResponse<Moto>.SuccessResponse(motoExistenteChassi, "Moto existente alocada ao pátio com sucesso");
                    HateoasHelper.AddHateoasLinks(responseUpdate, "Motos", motoExistenteChassi.Id, Url);
                    
                    return Ok(responseUpdate);
                }

                // Se não existe, criar nova moto
                var moto = new Moto(motoDto.Modelo, motoDto.Placa, motoDto.Chassi, motoDto.PatioId)
                {
                    AnoFabricacao = motoDto.AnoFabricacao,
                    Cor = motoDto.Cor
                };
                
                _context.Motos.Add(moto);
                await _context.SaveChangesAsync();

                // Carregar o pátio
                await _context.Entry(moto).Reference(m => m.Patio).LoadAsync();

                var response = ApiResponse<Moto>.SuccessResponse(moto, "Moto criada com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Motos", moto.Id, Url);

                return CreatedAtAction(nameof(GetById), new { id = moto.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Moto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Atualiza uma moto existente
        /// </summary>
        /// <param name="id">ID da moto</param>
        /// <param name="motoDto">Dados atualizados da moto</param>
        /// <returns>Moto atualizada</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Moto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<Moto>>> Update(int id, [FromBody] UpdateMotoDto motoDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<Moto>.ErrorResponse("ID deve ser maior que zero"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<Moto>.ErrorResponse("Dados inválidos", errors));
                }

                var moto = await _context.Motos.Include(m => m.Patio).FirstOrDefaultAsync(m => m.Id == id);
                if (moto == null)
                {
                    return NotFound(ApiResponse<Moto>.ErrorResponse("Moto não encontrada"));
                }

                // Verificar se o novo pátio existe (se fornecido)
                if (motoDto.PatioId.HasValue)
                {
                    var patio = await _context.Patios.FindAsync(motoDto.PatioId.Value);
                    if (patio == null)
                    {
                        return NotFound(ApiResponse<Moto>.ErrorResponse("Pátio especificado não encontrado"));
                    }
                }

                // Verificar duplicação de placa
                if (!string.IsNullOrWhiteSpace(motoDto.Placa) && 
                    motoDto.Placa.ToUpper() != moto.Placa.ToUpper())
                {
                    var existingMoto = await _context.Motos
                        .FirstOrDefaultAsync(m => m.Placa.ToUpper() == motoDto.Placa.ToUpper() && m.Id != id);
                    
                    if (existingMoto != null)
                    {
                        return Conflict(ApiResponse<Moto>.ErrorResponse("Já existe outra moto com esta placa"));
                    }
                }

                // Verificar duplicação de chassi
                if (!string.IsNullOrWhiteSpace(motoDto.Chassi) && 
                    motoDto.Chassi.ToUpper() != moto.Chassi.ToUpper())
                {
                    var existingMoto = await _context.Motos
                        .FirstOrDefaultAsync(m => m.Chassi.ToUpper() == motoDto.Chassi.ToUpper() && m.Id != id);
                    
                    if (existingMoto != null)
                    {
                        return Conflict(ApiResponse<Moto>.ErrorResponse("Já existe outra moto com este chassi"));
                    }
                }

                // Atualizar propriedades
                if (!string.IsNullOrWhiteSpace(motoDto.Modelo))
                    moto.Modelo = motoDto.Modelo;
                
                if (!string.IsNullOrWhiteSpace(motoDto.Placa))
                    moto.Placa = motoDto.Placa;
                
                if (!string.IsNullOrWhiteSpace(motoDto.Chassi))
                    moto.Chassi = motoDto.Chassi;

                if (motoDto.PatioId.HasValue)
                    moto.PatioId = motoDto.PatioId.Value;

                if (motoDto.AnoFabricacao.HasValue)
                    moto.AnoFabricacao = motoDto.AnoFabricacao;

                if (!string.IsNullOrWhiteSpace(motoDto.Cor))
                    moto.Cor = motoDto.Cor;

                if (motoDto.Ativo.HasValue)
                    moto.Ativo = motoDto.Ativo.Value;

                moto.DataAtualizacao = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Recarregar com o pátio atualizado
                await _context.Entry(moto).Reference(m => m.Patio).LoadAsync();

                var response = ApiResponse<Moto>.SuccessResponse(moto, "Moto atualizada com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Motos", moto.Id, Url);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Moto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Remove uma moto
        /// </summary>
        /// <param name="id">ID da moto</param>
        /// <returns>Confirmação de remoção</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("ID deve ser maior que zero"));
                }

                var moto = await _context.Motos.FindAsync(id);
                if (moto == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Moto não encontrada"));
                }

                _context.Motos.Remove(moto);
                await _context.SaveChangesAsync();

                var response = ApiResponse<object>.SuccessResponse(new object(), "Moto removida com sucesso");
                response.Links.Add(new Link(Url.Action(nameof(GetAll), "Motos")!, "list"));

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }
    }

    // DTOs para as operações
    public class CreateMotoDto
    {
        [Required(ErrorMessage = "Modelo é obrigatório")]
        [StringLength(100, ErrorMessage = "Modelo deve ter no máximo 100 caracteres")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Placa é obrigatória")]
        [StringLength(10, ErrorMessage = "Placa deve ter no máximo 10 caracteres")]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Chassi é obrigatório")]
        [StringLength(20, ErrorMessage = "Chassi deve ter no máximo 20 caracteres")]
        public string Chassi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pátio é obrigatório")]
        public int PatioId { get; set; }

        [Range(1900, 2100, ErrorMessage = "Ano de fabricação deve estar entre 1900 e 2100")]
        public int? AnoFabricacao { get; set; }

        [StringLength(50, ErrorMessage = "Cor deve ter no máximo 50 caracteres")]
        public string? Cor { get; set; }
    }

    public class UpdateMotoDto
    {
        [StringLength(100, ErrorMessage = "Modelo deve ter no máximo 100 caracteres")]
        public string? Modelo { get; set; }

        [StringLength(10, ErrorMessage = "Placa deve ter no máximo 10 caracteres")]
        public string? Placa { get; set; }

        [StringLength(20, ErrorMessage = "Chassi deve ter no máximo 20 caracteres")]
        public string? Chassi { get; set; }

        public int? PatioId { get; set; }

        [Range(1900, 2100, ErrorMessage = "Ano de fabricação deve estar entre 1900 e 2100")]
        public int? AnoFabricacao { get; set; }

        [StringLength(50, ErrorMessage = "Cor deve ter no máximo 50 caracteres")]
        public string? Cor { get; set; }

        public bool? Ativo { get; set; }
    }
}
