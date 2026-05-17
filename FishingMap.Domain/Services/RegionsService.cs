using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.DTO.Regions;
using FishingMap.Domain.Interfaces;
using MapsterMapper;

namespace FishingMap.Domain.Services
{
    public class RegionsService : IRegionsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RegionsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RegionDTO>> GetRegions()
        {
            var regions = await _unitOfWork.Regions.GetAll(orderBy: q => q.OrderBy(r => r.Name));
            return _mapper.Map<IEnumerable<RegionDTO>>(regions);
        }

        public async Task<RegionDTO?> GetRegion(int id)
        {
            var region = await _unitOfWork.Regions.GetById(id, noTracking: true);
            return region == null ? null : _mapper.Map<RegionDTO>(region);
        }

        public async Task<RegionDTO> AddRegion(RegionAdd region)
        {
            if (region.Type == RegionType.National)
            {
                if (await _unitOfWork.Regions.Any(r => r.Type == RegionType.National))
                {
                    throw new ArgumentException("A National region already exists. Only one is allowed.");
                }
                if (region.ParentRegionId.HasValue)
                {
                    throw new ArgumentException("National region cannot have a parent.");
                }
            }
            else if (!region.ParentRegionId.HasValue)
            {
                throw new ArgumentException("Non-national regions must have a parent.");
            }
            else if (!await _unitOfWork.Regions.Any(r => r.Id == region.ParentRegionId.Value))
            {
                throw new ArgumentException($"Parent region with id {region.ParentRegionId.Value} not found.");
            }

            var now = DateTime.Now;
            var entity = new Region
            {
                Name = region.Name,
                Type = region.Type,
                ParentRegionId = region.ParentRegionId,
                Created = now,
                Modified = now
            };

            entity = _unitOfWork.Regions.Add(entity);
            await _unitOfWork.SaveChanges();
            return _mapper.Map<RegionDTO>(entity);
        }

        public async Task<RegionDTO> UpdateRegion(int id, RegionUpdate region)
        {
            var entity = await _unitOfWork.Regions.GetById(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Region with id {id} not found.");
            }

            if (entity.Type == RegionType.National)
            {
                if (region.Type != RegionType.National)
                {
                    throw new ArgumentException("National region's type cannot be changed.");
                }
                if (region.ParentRegionId.HasValue)
                {
                    throw new ArgumentException("National region cannot have a parent.");
                }
            }
            else
            {
                if (region.Type == RegionType.National)
                {
                    throw new ArgumentException("Cannot promote a region to National. The National row is fixed.");
                }
                if (!region.ParentRegionId.HasValue)
                {
                    throw new ArgumentException("Non-national regions must have a parent.");
                }
                if (region.ParentRegionId.Value == id)
                {
                    throw new ArgumentException("Region cannot be its own parent.");
                }
                if (!await _unitOfWork.Regions.Any(r => r.Id == region.ParentRegionId.Value))
                {
                    throw new ArgumentException($"Parent region with id {region.ParentRegionId.Value} not found.");
                }
            }

            entity.Name = region.Name;
            entity.Type = region.Type;
            entity.ParentRegionId = region.ParentRegionId;
            entity.Modified = DateTime.Now;

            await _unitOfWork.SaveChanges();
            return _mapper.Map<RegionDTO>(entity);
        }

        public async Task DeleteRegion(int id)
        {
            var entity = await _unitOfWork.Regions.GetById(id);
            if (entity == null) return;

            if (entity.Type == RegionType.National)
            {
                throw new ArgumentException("National region cannot be deleted.");
            }

            if (await _unitOfWork.Regions.Any(r => r.ParentRegionId == id))
            {
                throw new ArgumentException("Region has child regions. Reassign or delete them first.");
            }

            _unitOfWork.Regions.Delete(entity);
            await _unitOfWork.SaveChanges();
        }
    }
}
