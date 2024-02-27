using AutoMapper;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.DTO.Permits;
using FishingMap.Domain.Interfaces;

namespace FishingMap.Domain.Services
{
    public class PermitsService : IPermitsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PermitsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PermitDTO> AddPermit(PermitAdd permit)
        {
            var now = DateTime.Now;
            var entity = new Permit()
            {
                Name = permit.Name,
                Url = permit.Url,
                Created = now,
                Modified = now
            };

            entity = _unitOfWork.Permits.Add(entity);
            await _unitOfWork.SaveChanges();

            return _mapper.Map<PermitDTO>(entity);
        }

        public async Task DeletePermit(int id)
        {
            await _unitOfWork.Permits.Delete(id);
            await _unitOfWork.SaveChanges();
        }

        public async Task<PermitDTO?> GetPermit(int id)
        {
            var permit = await _unitOfWork.Permits.GetById(id, noTracking: true);
            if (permit != null)
            {
                return _mapper.Map<PermitDTO>(permit);
            }

            return null;
        }

        public async Task<IEnumerable<PermitDTO>> GetPermits(string search)
        {
            var permits = await _unitOfWork.Permits.FindPermits(search);
            return _mapper.Map<IEnumerable<PermitDTO>>(permits);
        }

        public async Task<PermitDTO> UpdatePermit(int id, PermitUpdate permit)
        {
            var entity = await _unitOfWork.Permits.GetById(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Permit with id {id} not found.");
            }

            entity.Name = permit.Name;
            entity.Url = permit.Url;
            entity.Modified = DateTime.Now;

            await _unitOfWork.SaveChanges();

            return _mapper.Map<PermitDTO>(entity);
        }
    }
}
