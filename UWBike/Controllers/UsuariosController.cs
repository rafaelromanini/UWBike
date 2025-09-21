using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UWBike.Connection;
using UWBike.Model;
using UWBike.Common;
using System.ComponentModel.DataAnnotations;

namespace UWBike.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todos os usuários com paginação
        /// </summary>
        /// <param name="parameters">Parâmetros de paginação</param>
        /// <returns>Lista paginada de usuários</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<Usuario>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PagedResult<Usuario>>> GetAll([FromQuery] PaginationParameters parameters)
        {
            try
            {
                var query = _context.Usuarios.AsQueryable();

                // Filtro de busca por nome ou email
                if (!string.IsNullOrWhiteSpace(parameters.Search))
                {
                    query = query.Where(u => u.Nome.Contains(parameters.Search) || 
                                           u.Email.Contains(parameters.Search));
                }

                // Ordenação
                if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                {
                    switch (parameters.SortBy.ToLower())
                    {
                        case "nome":
                            query = parameters.SortDescending ? 
                                query.OrderByDescending(u => u.Nome) : 
                                query.OrderBy(u => u.Nome);
                            break;
                        case "email":
                            query = parameters.SortDescending ? 
                                query.OrderByDescending(u => u.Email) : 
                                query.OrderBy(u => u.Email);
                            break;
                        case "datacriacao":
                            query = parameters.SortDescending ? 
                                query.OrderByDescending(u => u.DataCriacao) : 
                                query.OrderBy(u => u.DataCriacao);
                            break;
                        default:
                            query = query.OrderBy(u => u.Id);
                            break;
                    }
                }
                else
                {
                    query = query.OrderBy(u => u.Id);
                }

                var totalRecords = await query.CountAsync();
                var usuarios = await query
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync();

                var pagedResult = new PagedResult<Usuario>(usuarios, parameters.PageNumber, parameters.PageSize, totalRecords);
                
                // Adicionar links HATEOAS para paginação
                HateoasHelper.AddPaginationLinks(
                    pagedResult, 
                    nameof(GetAll), 
                    "Usuarios", 
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
        /// Obtém um usuário específico por ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <returns>Dados do usuário</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Usuario>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<Usuario>>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<Usuario>.ErrorResponse("ID deve ser maior que zero"));
                }

                var usuario = await _context.Usuarios.FindAsync(id);
                
                if (usuario == null)
                {
                    return NotFound(ApiResponse<Usuario>.ErrorResponse("Usuário não encontrado"));
                }

                var response = ApiResponse<Usuario>.SuccessResponse(usuario, "Usuário encontrado com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Usuarios", id, Url);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Usuario>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtém um usuário por email
        /// </summary>
        /// <param name="email">Email do usuário</param>
        /// <returns>Dados do usuário</returns>
        [HttpGet("buscar")]
        [ProducesResponseType(typeof(ApiResponse<Usuario>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<Usuario>>> GetByEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(ApiResponse<Usuario>.ErrorResponse("Email é obrigatório"));
                }

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                
                if (usuario == null)
                {
                    return NotFound(ApiResponse<Usuario>.ErrorResponse("Usuário não encontrado"));
                }

                var response = ApiResponse<Usuario>.SuccessResponse(usuario, "Usuário encontrado com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Usuarios", usuario.Id, Url);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Usuario>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cria um novo usuário
        /// </summary>
        /// <param name="usuarioDto">Dados do usuário a ser criado</param>
        /// <returns>Usuário criado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Usuario>), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<Usuario>>> Create([FromBody] CreateUsuarioDto usuarioDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<Usuario>.ErrorResponse("Dados inválidos", errors));
                }

                // Verificar se já existe usuário com este email
                var existingUser = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == usuarioDto.Email.ToLower());
                
                if (existingUser != null)
                {
                    return Conflict(ApiResponse<Usuario>.ErrorResponse("Já existe um usuário com este email"));
                }

                var usuario = new Usuario(usuarioDto.Nome, usuarioDto.Email, usuarioDto.Senha);
                
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var response = ApiResponse<Usuario>.SuccessResponse(usuario, "Usuário criado com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Usuarios", usuario.Id, Url);

                return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Usuario>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Atualiza um usuário existente
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <param name="usuarioDto">Dados atualizados do usuário</param>
        /// <returns>Usuário atualizado</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Usuario>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse<Usuario>>> Update(int id, [FromBody] UpdateUsuarioDto usuarioDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<Usuario>.ErrorResponse("ID deve ser maior que zero"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<Usuario>.ErrorResponse("Dados inválidos", errors));
                }

                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return NotFound(ApiResponse<Usuario>.ErrorResponse("Usuário não encontrado"));
                }

                // Verificar se o email já está sendo usado por outro usuário
                if (!string.IsNullOrWhiteSpace(usuarioDto.Email) && 
                    usuarioDto.Email.ToLower() != usuario.Email.ToLower())
                {
                    var existingUser = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Email.ToLower() == usuarioDto.Email.ToLower() && u.Id != id);
                    
                    if (existingUser != null)
                    {
                        return Conflict(ApiResponse<Usuario>.ErrorResponse("Já existe outro usuário com este email"));
                    }
                }

                // Atualizar propriedades
                if (!string.IsNullOrWhiteSpace(usuarioDto.Nome))
                    usuario.Nome = usuarioDto.Nome;
                
                if (!string.IsNullOrWhiteSpace(usuarioDto.Email))
                    usuario.Email = usuarioDto.Email;
                
                if (!string.IsNullOrWhiteSpace(usuarioDto.Senha))
                    usuario.Senha = usuarioDto.Senha;

                usuario.DataAtualizacao = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var response = ApiResponse<Usuario>.SuccessResponse(usuario, "Usuário atualizado com sucesso");
                HateoasHelper.AddHateoasLinks(response, "Usuarios", usuario.Id, Url);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Usuario>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Remove um usuário
        /// </summary>
        /// <param name="id">ID do usuário</param>
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

                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Usuário não encontrado"));
                }

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                var response = ApiResponse<object>.SuccessResponse(new object(), "Usuário removido com sucesso");
                response.Links.Add(new Link(Url.Action(nameof(GetAll), "Usuarios")!, "list"));

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Erro interno do servidor: {ex.Message}"));
            }
        }
    }

    // DTOs para as operações
    public class CreateUsuarioDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(150, ErrorMessage = "Email deve ter no máximo 150 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 255 caracteres")]
        public string Senha { get; set; } = string.Empty;
    }

    public class UpdateUsuarioDto
    {
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string? Nome { get; set; }

        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(150, ErrorMessage = "Email deve ter no máximo 150 caracteres")]
        public string? Email { get; set; }

        [StringLength(255, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 255 caracteres")]
        public string? Senha { get; set; }
    }
}