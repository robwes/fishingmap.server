using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.DTO.Regulations;
using FishingMap.Domain.DTO.Species;
using FishingMap.Domain.Interfaces;
using MapsterMapper;

namespace FishingMap.Domain.Services
{
    public class RegulationsService : IRegulationsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RegulationsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SpeciesRegulationDTO>> GetRegulations()
        {
            var regulations = await _unitOfWork.SpeciesRegulations.GetAll(
                includeProperties: [r => r.Locations, r => r.ProtectedPeriods],
                noTracking: true);
            return _mapper.Map<IEnumerable<SpeciesRegulationDTO>>(regulations);
        }

        public async Task<SpeciesRegulationDTO?> GetRegulation(int id)
        {
            var regulation = await _unitOfWork.SpeciesRegulations.GetById(
                id,
                [r => r.Locations, r => r.ProtectedPeriods],
                noTracking: true);
            return regulation == null ? null : _mapper.Map<SpeciesRegulationDTO>(regulation);
        }

        public async Task<SpeciesRegulationDTO> AddRegulation(SpeciesRegulationAdd input)
        {
            await ValidateScope(input.RegionId, input.LocationIds, currentRegulationId: null, input.SpeciesId);

            if (!await _unitOfWork.Species.Any(s => s.Id == input.SpeciesId))
            {
                throw new ArgumentException($"Species with id {input.SpeciesId} not found.");
            }

            var now = DateTime.UtcNow;
            var entity = new SpeciesRegulation
            {
                SpeciesId = input.SpeciesId,
                RegionId = input.RegionId,
                MinimumSizeCm = input.MinimumSizeCm,
                MaximumSizeCm = input.MaximumSizeCm,
                BagLimit = input.BagLimit,
                BagLimitBasis = input.BagLimitBasis,
                IsCatchAndReleaseOnly = input.IsCatchAndReleaseOnly,
                MustReportCatch = input.MustReportCatch,
                AdditionalRules = input.AdditionalRules,
                Created = now,
                Modified = now,
                ProtectedPeriods = input.ProtectedPeriods.Select(p => new ProtectedPeriod
                {
                    StartMonth = p.StartMonth,
                    StartDay = p.StartDay,
                    EndMonth = p.EndMonth,
                    EndDay = p.EndDay,
                    Created = now,
                    Modified = now
                }).ToList()
            };

            await AttachLocations(entity, input.LocationIds);

            entity = _unitOfWork.SpeciesRegulations.Add(entity);
            await _unitOfWork.SaveChanges();

            return _mapper.Map<SpeciesRegulationDTO>(entity);
        }

        public async Task<SpeciesRegulationDTO> UpdateRegulation(int id, SpeciesRegulationUpdate input)
        {
            var entity = await _unitOfWork.SpeciesRegulations.GetById(
                id,
                [r => r.Locations, r => r.ProtectedPeriods]);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Regulation with id {id} not found.");
            }

            await ValidateScope(input.RegionId, input.LocationIds, currentRegulationId: id, input.SpeciesId);

            entity.SpeciesId = input.SpeciesId;
            entity.RegionId = input.RegionId;
            entity.MinimumSizeCm = input.MinimumSizeCm;
            entity.MaximumSizeCm = input.MaximumSizeCm;
            entity.BagLimit = input.BagLimit;
            entity.BagLimitBasis = input.BagLimitBasis;
            entity.IsCatchAndReleaseOnly = input.IsCatchAndReleaseOnly;
            entity.MustReportCatch = input.MustReportCatch;
            entity.AdditionalRules = input.AdditionalRules;
            entity.Modified = DateTime.UtcNow;

            entity.Locations.Clear();
            await AttachLocations(entity, input.LocationIds);

            entity.ProtectedPeriods.Clear();
            var now = DateTime.UtcNow;
            foreach (var p in input.ProtectedPeriods)
            {
                entity.ProtectedPeriods.Add(new ProtectedPeriod
                {
                    StartMonth = p.StartMonth,
                    StartDay = p.StartDay,
                    EndMonth = p.EndMonth,
                    EndDay = p.EndDay,
                    Created = now,
                    Modified = now
                });
            }

            await _unitOfWork.SaveChanges();
            return _mapper.Map<SpeciesRegulationDTO>(entity);
        }

        public async Task DeleteRegulation(int id)
        {
            var entity = await _unitOfWork.SpeciesRegulations.GetById(id);
            if (entity == null) return;

            _unitOfWork.SpeciesRegulations.Delete(entity);
            await _unitOfWork.SaveChanges();
        }

        public async Task<IEnumerable<LocationSpeciesRuleDTO>> GetEffectiveRulesForLocation(int locationId)
        {
            var location = await _unitOfWork.Locations.GetById(locationId, noTracking: true);
            if (location == null)
            {
                throw new KeyNotFoundException($"Location with id {locationId} not found.");
            }

            var ancestorIds = await BuildAncestorChain(location.RegionId);

            var candidates = await _unitOfWork.SpeciesRegulations.GetCandidatesForLocation(locationId, ancestorIds);

            int Rank(SpeciesRegulation r) =>
                r.Locations.Any(l => l.Id == locationId) ? 0
                : 1 + ancestorIds.IndexOf(r.RegionId!.Value);

            return candidates
                .GroupBy(r => r.SpeciesId)
                .Select(g =>
                {
                    // Rank ascending = most specific first. The winner is the rule that
                    // applies; the runner-up is what a maintainer would fall back to if the
                    // winner were removed, which the editor needs in order to say so.
                    var ordered = g.OrderBy(Rank).ToList();
                    var dto = ToRule(ordered[0], locationId);
                    dto.FallsBackTo = ordered.Count > 1 ? ToRule(ordered[1], locationId) : null;
                    return dto;
                })
                .ToList();
        }

        private LocationSpeciesRuleDTO ToRule(SpeciesRegulation regulation, int locationId)
        {
            var dto = _mapper.Map<LocationSpeciesRuleDTO>(regulation);
            dto.Source = ResolveSource(regulation, locationId);
            return dto;
        }

        public async Task<IEnumerable<SpeciesRegulationScopeDTO>> GetRegulationsForSpecies(int speciesId)
        {
            if (!await _unitOfWork.Species.Any(s => s.Id == speciesId))
            {
                throw new KeyNotFoundException($"Species with id {speciesId} not found.");
            }

            var regulations = await _unitOfWork.SpeciesRegulations.GetForSpecies(speciesId);

            // National first, then the rest of the tree by tier and name, then the
            // location-scoped rules — the order the species details screen reads them in.
            return regulations
                .Select(r => _mapper.Map<SpeciesRegulationScopeDTO>(r))
                .OrderBy(r => r.Region == null ? 1 : 0)
                .ThenBy(r => r.Region == null ? 0 : (int)r.Region.Type)
                .ThenBy(r => r.Region?.Name ?? string.Empty)
                .ThenBy(r => r.Locations.Select(l => l.Name).FirstOrDefault() ?? string.Empty)
                .ToList();
        }

        private static string ResolveSource(SpeciesRegulation r, int locationId)
        {
            if (r.Locations.Any(l => l.Id == locationId))
            {
                return "Location";
            }
            if (r.Region != null)
            {
                return r.Region.Type == RegionType.National
                    ? "National"
                    : $"Region: {r.Region.Name}";
            }
            return "Unknown";
        }

        private async Task<List<int>> BuildAncestorChain(int? regionId)
        {
            if (regionId.HasValue)
            {
                var chain = await _unitOfWork.Regions.GetAncestry(regionId.Value);
                return chain.Select(r => r.Id).ToList();
            }

            var nationalId = await _unitOfWork.Regions.GetNationalRegionId();
            return new List<int> { nationalId };
        }

        private async Task ValidateScope(int? regionId, IEnumerable<int> locationIds, int? currentRegulationId, int speciesId)
        {
            var locationIdList = locationIds?.Distinct().ToList() ?? new List<int>();
            var hasRegion = regionId.HasValue;
            var hasLocations = locationIdList.Count > 0;

            if (hasRegion && hasLocations)
            {
                throw new ArgumentException("A regulation cannot have both RegionId and LocationIds set.");
            }
            if (!hasRegion && !hasLocations)
            {
                throw new ArgumentException("A regulation must specify either a RegionId (use the National region for nationwide rules) or one or more LocationIds.");
            }

            if (hasRegion && !await _unitOfWork.Regions.Any(r => r.Id == regionId!.Value))
            {
                throw new ArgumentException($"Region with id {regionId!.Value} not found.");
            }

            if (hasLocations)
            {
                var conflict = await _unitOfWork.SpeciesRegulations.Find(
                    r => r.Id != (currentRegulationId ?? 0)
                         && r.SpeciesId == speciesId
                         && r.Locations.Any(l => locationIdList.Contains(l.Id)));
                if (conflict != null)
                {
                    throw new ArgumentException("One or more locations already have a location-scoped rule for this species.");
                }
            }
        }

        private async Task AttachLocations(SpeciesRegulation regulation, IEnumerable<int> locationIds)
        {
            var ids = locationIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0) return;

            var locations = await _unitOfWork.Locations.GetAll(l => ids.Contains(l.Id));
            foreach (var location in locations)
            {
                regulation.Locations.Add(location);
            }
        }

    }
}
