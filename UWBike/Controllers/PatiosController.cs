using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UWBike.Connection;
using UWBike.Model;
using UWBike.Common;
using System.ComponentModel.DataAnnotations;
using DTOs;

namespace UWBike.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PatiosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PatiosController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todos os pátios com paginação
        /// </summary>
        /// <param name="parameters">Parâmetros de paginação</param>
        /// <returns>Lista paginada de pátios</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<PatioDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PagedResult<PatioDto>>> GetAll([FromQuery] PaginationParameters parameters)
        {
            try
            {
                var query = _context.Patios.Include(p => p.Motos).AsQueryable();

                // Filtro de busca por nome, cidade ou endereço
                if (!string.IsNullOrWhiteSpace(parameters.Search))
                {
                    query = query.Where(p => p.Nome.Contains(parameters.Search) ||
                                           (p.Cidade != null && p.Cidade.Contains(parameters.Search)) ||
                                           p.Endereco.Contains(parameters.Search));
                }

                // Ordenação
                if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                {
                    switch (parameters.SortBy.ToLower())
                    {
                        case "nome":
                            query = parameters.SortDescending ?
                                query.OrderByDescending(p => p.Nome) :
                                query.OrderBy(p => p.Nome);
                            break;
                        case "cidade":
                            query = parameters.SortDescending ?
                                query.OrderByDescending(p => p.Cidade) :
                                query.OrderBy(p => p.Cidade);
                            break;
                        case "capacidade":
                            query = parameters.SortDescending ?
                                query.OrderByDescending(p => p.Capacidade) :
                                query.OrderBy(p => p.Capacidade);
                            break;
                        case "datacriacao":
                            query = parameters.SortDescending ?
                                query.OrderByDescending(p => p.DataCriacao) :
                                query.OrderBy(p => p.DataCriacao);
                            break;
                        default:
                            query = query.OrderBy(p => p.Id);
                            break;
                    }
                }
                else
                {
                    query = query.OrderBy(p => p.Id);
                }

                var totalRecords = await query.CountAsync();
                var patios = await query
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync();

                var pagedResult = new PagedResult<PatioDto>([.. patios.Select(PatioDto.fromPatio)], parameters.PageNumber, parameters.PageSize, totalRecords);

                // Adicionar links HATEOAS para paginação
                HateoasHelper.AddPaginationLinks(
                    pagedResult,
                    nameof(GetAll),
                    "Patios",
                    Url,
                    new
                    {
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
        /// Obtém um pátio específico por ID
        /// </summary>
        /// <param name="id">ID do pátio</param>
        /// <returns>Dados do pátio</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<PatioDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<PatioDto>>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<PatioDto>.ErrorResponse("ID deve ser maior que zero"));
                }

                var patio = await _context.Patios
                    .Include(p => p.Motos)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (patio == null)
                {
                    return NotFound(ApiResponse<PatioDto>.ErrorResponse("Pátio não encontrado"));
                }

                var response = ApiResponse<PatioDto>.SuccessResponse(PatioDto.fromPatio(patio), "Pátio encontrado com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Patios", id, Url);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PatioDto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtém as motos de um pátio específico
        /// </summary>
        /// <param name="id">ID do pátio</param>
        /// <param name="parameters">Parâmetros de paginação</param>
        /// <returns>Lista paginada de motos do pátio</returns>
        [HttpGet("{id:int}/motos")]
        [ProducesResponseType(typeof(PagedResult<Moto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PagedResult<Moto>>> GetMotos(int id, [FromQuery] PaginationParameters parameters)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("ID deve ser maior que zero"));
                }

                var patio = await _context.Patios.FindAsync(id);
                if (patio == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Pátio não encontrado"));
                }

                var query = _context.Motos
                    .Where(m => m.PatioId == id)
                    .Include(m => m.Patio)
                    .AsQueryable();

                // Filtro de busca por modelo, placa ou chassi
                if (!string.IsNullOrWhiteSpace(parameters.Search))
                {
                    query = query.Where(m => m.Modelo.Contains(parameters.Search) ||
                                           m.Placa.Contains(parameters.Search) ||
                                           m.Chassi.Contains(parameters.Search));
                }

                var totalRecords = await query.CountAsync();
                var motos = await query
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync();

                var pagedResult = new PagedResult<MotoDto>([.. motos.Select(MotoDto.fromMoto)], parameters.PageNumber, parameters.PageSize, totalRecords);

                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cria um novo pátio
        /// </summary>
        /// <param name="patioDto">Dados do pátio a ser criado</param>
        /// <returns>Pátio criado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatioDto>), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<PatioDto>>> Create([FromBody] CreatePatioDto patioDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<PatioDto>.ErrorResponse("Dados inválidos", errors));
                }

                var patio = new Patio(patioDto.Nome, patioDto.Endereco, patioDto.Capacidade)
                {
                    Cep = patioDto.Cep,
                    Cidade = patioDto.Cidade,
                    Estado = patioDto.Estado,
                    Telefone = patioDto.Telefone
                };

                _context.Patios.Add(patio);
                await _context.SaveChangesAsync();

                var response = ApiResponse<PatioDto>.SuccessResponse(PatioDto.fromPatio(patio), "Pátio criado com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Patios", patio.Id, Url);

                return CreatedAtAction(nameof(GetById), new { id = patio.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PatioDto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Atualiza um pátio existente
        /// </summary>
        /// <param name="id">ID do pátio</param>
        /// <param name="patioDto">Dados atualizados do pátio</param>
        /// <returns>Pátio atualizado</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<PatioDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<PatioDto>>> Update(int id, [FromBody] UpdatePatioDto patioDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<PatioDto>.ErrorResponse("ID deve ser maior que zero"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<PatioDto>.ErrorResponse("Dados inválidos", errors));
                }

                var patio = await _context.Patios.FindAsync(id);
                if (patio == null)
                {
                    return NotFound(ApiResponse<PatioDto>.ErrorResponse("Pátio não encontrado"));
                }

                // Atualizar propriedades
                if (!string.IsNullOrWhiteSpace(patioDto.Nome))
                    patio.Nome = patioDto.Nome;

                if (!string.IsNullOrWhiteSpace(patioDto.Endereco))
                    patio.Endereco = patioDto.Endereco;

                if (patioDto.Capacidade.HasValue && patioDto.Capacidade > 0)
                    patio.Capacidade = patioDto.Capacidade.Value;

                if (!string.IsNullOrWhiteSpace(patioDto.Cep))
                    patio.Cep = patioDto.Cep;

                if (!string.IsNullOrWhiteSpace(patioDto.Cidade))
                    patio.Cidade = patioDto.Cidade;

                if (!string.IsNullOrWhiteSpace(patioDto.Estado))
                    patio.Estado = patioDto.Estado;

                if (!string.IsNullOrWhiteSpace(patioDto.Telefone))
                    patio.Telefone = patioDto.Telefone;

                if (patioDto.Ativo.HasValue)
                    patio.Ativo = patioDto.Ativo.Value;

                patio.DataAtualizacao = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var response = ApiResponse<PatioDto>.SuccessResponse(PatioDto.fromPatio(patio), "Pátio atualizado com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Patios", patio.Id, Url);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PatioDto>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Remove um pátio
        /// </summary>
        /// <param name="id">ID do pátio</param>
        /// <returns>Confirmação de remoção</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("ID deve ser maior que zero"));
                }

                var patio = await _context.Patios
                    .Include(p => p.Motos)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (patio == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Pátio não encontrado"));
                }

                // Verificar se o pátio tem motos associadas
                if (patio.Motos.Any())
                {
                    return Conflict(ApiResponse<object>.ErrorResponse($"Não é possível remover o pátio porque ele possui {patio.Motos.Count} moto(s) associada(s)"));
                }

                _context.Patios.Remove(patio);
                await _context.SaveChangesAsync();

                var response = ApiResponse<object>.SuccessResponse(new object(), "Pátio removido com sucesso");
                response.Links.Add(new Link(Url.Action(nameof(GetAll), "Patios")!, "list"));

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }
    }
}