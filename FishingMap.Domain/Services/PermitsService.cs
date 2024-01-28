using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.Data.DTO.PermitObjects;
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

        public async Task<PermitDTO> AddPermit(PermitDTO permit)
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

        public async Task<PermitDTO> GetPermit(int id)
        {
            var permit = await _unitOfWork.Permits.GetById(id);
            if (permit != null)
            {
                return _mapper.Map<PermitDTO>(permit);
            }

            return null;
        }

        public async Task<IEnumerable<PermitDTO>> GetPermits(string search)
        {
            Expression<Func<Permit, bool>> query = null;
            if (!string.IsNullOrEmpty(search))
            {
                query = p => p.Name.Contains(search);
            }

            var permits = await _unitOfWork.Permits.GetAll(
                query,
                orderBy: p => p.OrderBy(p => p.Name));

            return _mapper.Map<IEnumerable<PermitDTO>>(permits);
        }

        public async Task<PermitDTO> UpdatePermit(int id, PermitDTO permit)
        {
            var entity = await _unitOfWork.Permits.GetById(id);
            if (entity != null)
            {
                entity.Name = permit.Name;
                entity.Url = permit.Url;
                entity.Modified = DateTime.Now;

                entity = _unitOfWork.Permits.Update(entity);
                await _unitOfWork.SaveChanges();

                return _mapper.Map<PermitDTO>(entity);
            }

            return null;
        }
    }
}
