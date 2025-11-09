using UWBike.Interfaces;
using UWBike.Model;
using UWBike.Common;
using DTOs;

namespace UWBike.Services
{
    public class PatioService : IPatioService
    {
        private readonly IPatioRepository _patioRepository;

        public PatioService(IPatioRepository patioRepository)
        {
            _patioRepository = patioRepository;
        }

        public async Task<PagedResult<PatioDto>> GetAllAsync(PaginationParameters parameters)
        {
            var (patios, totalRecords) = await _patioRepository.GetPagedAsync(parameters);
            var patioDtos = patios.Select(PatioDto.fromPatio).ToList();
            
            return new PagedResult<PatioDto>(patioDtos, parameters.PageNumber, parameters.PageSize, totalRecords);
        }

        public async Task<PatioDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero");

            var patio = await _patioRepository.GetByIdAsync(id);
            return patio == null ? null : PatioDto.fromPatio(patio);
        }

        public async Task<List<PatioDto>> GetByIdOrNameAsync(string identificador)
        {
            if (string.IsNullOrWhiteSpace(identificador))
                throw new ArgumentException("Identificador é obrigatório");

            List<PatioDto> patios = new List<PatioDto>();

            // Tentar converter para int (ID)
            if (int.TryParse(identificador, out int id))
            {
                var patio = await GetByIdAsync(id);
                if (patio != null)
                {
                    patios.Add(patio);
                }
            }
            else
            {
                // Buscar por nome usando paginação
                var todosPaginated = await GetAllAsync(new PaginationParameters
                {
                    Search = identificador,
                    PageNumber = 1,
                    PageSize = 100
                });

                patios = todosPaginated.Data.ToList();
            }

            return patios;
        }

        public async Task<PagedResult<MotoDto>> GetMotosFromPatioAsync(int patioId, PaginationParameters parameters)
        {
            if (patioId <= 0)
                throw new ArgumentException("ID deve ser maior que zero");

            var patio = await _patioRepository.GetByIdAsync(patioId);
            if (patio == null)
                throw new InvalidOperationException("Pátio não encontrado");

            var (motos, totalRecords) = await _patioRepository.GetMotosFromPatioAsync(patioId, parameters);
            var motoDtos = motos.Select(MotoDto.fromMoto).ToList();
            
            return new PagedResult<MotoDto>(motoDtos, parameters.PageNumber, parameters.PageSize, totalRecords);
        }

        public async Task<PatioDto> CreateAsync(CreatePatioDto patioDto)
        {
            var patio = new Patio(patioDto.Nome, patioDto.Endereco, patioDto.Capacidade)
            {
                Cep = patioDto.Cep,
                Cidade = patioDto.Cidade,
                Estado = patioDto.Estado,
                Telefone = patioDto.Telefone
            };

            var patioCriado = await _patioRepository.CreateAsync(patio);
            return PatioDto.fromPatio(patioCriado);
        }

        public async Task<PatioDto> UpdateAsync(int id, UpdatePatioDto patioDto)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero");

            var patio = await _patioRepository.GetByIdAsync(id);
            if (patio == null)
                throw new InvalidOperationException("Pátio não encontrado");

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

            var patioAtualizado = await _patioRepository.UpdateAsync(patio);
            return PatioDto.fromPatio(patioAtualizado);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero");

            var patio = await _patioRepository.GetByIdWithMotosAsync(id);
            if (patio == null)
                throw new InvalidOperationException("Pátio não encontrado");

            // REGRA DE NEGÓCIO: Verificar se o pátio tem motos associadas
            var hasMotos = await _patioRepository.HasMotosAsync(id);
            if (hasMotos)
            {
                throw new InvalidOperationException(
                    $"Não é possível remover o pátio porque ele possui moto(s) associada(s)");
            }

            return await _patioRepository.DeleteAsync(id);
        }
    }
}
