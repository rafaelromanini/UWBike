using UWBike.Interfaces;
using UWBike.Model;
using UWBike.Common;
using DTOs;

namespace UWBike.Services
{
    public class MotoService : IMotoService
    {
        private readonly IMotoRepository _motoRepository;
        private readonly IPatioRepository _patioRepository;

        public MotoService(IMotoRepository motoRepository, IPatioRepository patioRepository)
        {
            _motoRepository = motoRepository;
            _patioRepository = patioRepository;
        }

        public async Task<PagedResult<MotoDto>> GetAllAsync(PaginationParameters parameters)
        {
            var (motos, totalRecords) = await _motoRepository.GetPagedAsync(parameters);
            var motoDtos = motos.Select(MotoDto.fromMoto).ToList();
            
            return new PagedResult<MotoDto>(motoDtos, parameters.PageNumber, parameters.PageSize, totalRecords);
        }

        public async Task<MotoDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero");

            var moto = await _motoRepository.GetByIdAsync(id);
            return moto == null ? null : MotoDto.fromMoto(moto);
        }

        public async Task<MotoDto?> GetByPlacaAsync(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa))
                throw new ArgumentException("Placa é obrigatória");

            var moto = await _motoRepository.GetByPlacaAsync(placa);
            return moto == null ? null : MotoDto.fromMoto(moto);
        }

        public async Task<MotoDto> CreateAsync(CreateMotoDto motoDto)
        {
            // Validar se o pátio existe
            var patio = await _patioRepository.GetByIdAsync(motoDto.PatioId);
            if (patio == null)
                throw new InvalidOperationException("Pátio especificado não encontrado");

            // REGRA DE NEGÓCIO: Verificar se já existe moto com a mesma placa ou chassi
            var motoExistentePlaca = await _motoRepository.GetByPlacaAsync(motoDto.Placa);
            
            if (motoExistentePlaca != null)
            {
                // Se a moto já tem um pátio, não pode cadastrar novamente
                if (motoExistentePlaca.PatioId > 0)
                {
                    throw new InvalidOperationException(
                        $"Já existe uma moto com a placa {motoDto.Placa} alocada no pátio {motoExistentePlaca.Patio?.Nome}");
                }
                
                // Se não tem pátio, aloca ao pátio especificado
                motoExistentePlaca.PatioId = motoDto.PatioId;
                var motoAtualizada = await _motoRepository.UpdateAsync(motoExistentePlaca);
                return MotoDto.fromMoto(motoAtualizada);
            }

            var motoExistenteChassi = await _motoRepository.GetByChassiAsync(motoDto.Chassi);
            
            if (motoExistenteChassi != null)
            {
                if (motoExistenteChassi.PatioId > 0)
                {
                    throw new InvalidOperationException(
                        $"Já existe uma moto com o chassi {motoDto.Chassi} alocada no pátio {motoExistenteChassi.Patio?.Nome}");
                }
                
                motoExistenteChassi.PatioId = motoDto.PatioId;
                var motoAtualizada = await _motoRepository.UpdateAsync(motoExistenteChassi);
                return MotoDto.fromMoto(motoAtualizada);
            }

            // Criar nova moto
            var moto = new Moto(motoDto.Modelo, motoDto.Placa, motoDto.Chassi, motoDto.PatioId)
            {
                AnoFabricacao = motoDto.AnoFabricacao,
                Cor = motoDto.Cor
            };
            
            var motoCriada = await _motoRepository.CreateAsync(moto);
            return MotoDto.fromMoto(motoCriada);
        }

        public async Task<MotoDto> UpdateAsync(int id, UpdateMotoDto motoDto)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero");

            var moto = await _motoRepository.GetByIdAsync(id);
            if (moto == null)
                throw new InvalidOperationException("Moto não encontrada");

            // Verificar se o novo pátio existe (se fornecido)
            if (motoDto.PatioId.HasValue)
            {
                var patio = await _patioRepository.GetByIdAsync(motoDto.PatioId.Value);
                if (patio == null)
                    throw new InvalidOperationException("Pátio especificado não encontrado");
            }

            // REGRA DE NEGÓCIO: Verificar duplicação de placa
            if (!string.IsNullOrWhiteSpace(motoDto.Placa) && 
                motoDto.Placa.ToUpper() != moto.Placa.ToUpper())
            {
                var exists = await _motoRepository.ExistsByPlacaAsync(motoDto.Placa, id);
                if (exists)
                    throw new InvalidOperationException("Já existe outra moto com esta placa");
            }

            // REGRA DE NEGÓCIO: Verificar duplicação de chassi
            if (!string.IsNullOrWhiteSpace(motoDto.Chassi) && 
                motoDto.Chassi.ToUpper() != moto.Chassi.ToUpper())
            {
                var exists = await _motoRepository.ExistsByChassiAsync(motoDto.Chassi, id);
                if (exists)
                    throw new InvalidOperationException("Já existe outra moto com este chassi");
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

            var motoAtualizada = await _motoRepository.UpdateAsync(moto);
            return MotoDto.fromMoto(motoAtualizada);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID deve ser maior que zero");

            var moto = await _motoRepository.GetByIdAsync(id);
            if (moto == null)
                throw new InvalidOperationException("Moto não encontrada");

            return await _motoRepository.DeleteAsync(id);
        }
    }
}
