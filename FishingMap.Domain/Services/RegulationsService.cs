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
            await ValidateScope(input.RegionId, input.LocationIds, currentRegulationId: null, input.SpeciesId, input.AdiposeFin);
            ValidateProtectedPeriods(input.ProtectedPeriods);

            if (!await _unitOfWork.Species.Any(s => s.Id == input.SpeciesId))
            {
                throw new ArgumentException($"Species with id {input.SpeciesId} not found.");
            }

            var now = DateTime.UtcNow;
            var entity = new SpeciesRegulation
            {
                SpeciesId = input.SpeciesId,
                RegionId = input.RegionId,
                AdiposeFin = input.AdiposeFin,
                MinimumSizeCm = input.MinimumSizeCm,
                MaximumSizeCm = input.MaximumSizeCm,
                BagLimit = input.BagLimit,
                BagLimitBasis = input.BagLimitBasis,
                IsCatchAndReleaseOnly = input.IsCatchAndReleaseOnly,
                IsFullyProtected = input.IsFullyProtected,
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
            await ClearFollowsRegion(input.SpeciesId, input.LocationIds);

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

            await ValidateScope(input.RegionId, input.LocationIds, currentRegulationId: id, input.SpeciesId, input.AdiposeFin);
            ValidateProtectedPeriods(input.ProtectedPeriods);

            entity.SpeciesId = input.SpeciesId;
            entity.RegionId = input.RegionId;
            entity.AdiposeFin = input.AdiposeFin;
            entity.MinimumSizeCm = input.MinimumSizeCm;
            entity.MaximumSizeCm = input.MaximumSizeCm;
            entity.BagLimit = input.BagLimit;
            entity.BagLimitBasis = input.BagLimitBasis;
            entity.IsCatchAndReleaseOnly = input.IsCatchAndReleaseOnly;
            entity.IsFullyProtected = input.IsFullyProtected;
            entity.MustReportCatch = input.MustReportCatch;
            entity.AdditionalRules = input.AdditionalRules;
            entity.Modified = DateTime.UtcNow;

            entity.Locations.Clear();
            await AttachLocations(entity, input.LocationIds);
            await ClearFollowsRegion(input.SpeciesId, input.LocationIds);

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

            // Inheritance is opt-in. A region rule reaches this water only for species an
            // administrator chose to follow; for the rest it is discarded here, and the
            // species comes back with no rule at all rather than one nobody verified fits.
            // A location-scoped rule is not filtered — writing one IS the decision.
            // See decision 11 in robwes/fishingmap.web#13.
            var followed = await _unitOfWork.LocationSpeciesFollowsRegions.GetFollowedSpeciesIds(locationId);
            var applicable = candidates
                .Where(r => r.Locations.Any(l => l.Id == locationId) || followed.Contains(r.SpeciesId))
                .ToList();

            int Rank(SpeciesRegulation r) =>
                r.Locations.Any(l => l.Id == locationId) ? 0
                : 1 + ancestorIds.IndexOf(r.RegionId!.Value);

            var rules = new List<LocationSpeciesRuleDTO>();

            foreach (var speciesGroup in applicable.GroupBy(r => r.SpeciesId))
            {
                rules.AddRange(ResolveSpecies(speciesGroup, locationId, Rank));
            }

            return rules;
        }

        public async Task<IEnumerable<int>> GetFollowedSpeciesIds(int locationId)
        {
            return await _unitOfWork.LocationSpeciesFollowsRegions.GetFollowedSpeciesIds(locationId);
        }

        /// <summary>
        /// Makes a water inherit its region's rules for one species, or stops it.
        ///
        /// Following and having a custom rule are mutually exclusive, so this refuses to
        /// switch a species to following while a location-scoped rule for it still exists —
        /// the caller removes that first, which is the revert the editor already asks about.
        /// Silently deleting it here would throw away an authored rule on a toggle.
        /// </summary>
        /// <param name="locationId">The water.</param>
        /// <param name="speciesId">The species being decided.</param>
        /// <param name="follows">True to inherit, false to go back to undecided.</param>
        public async Task SetFollowsRegion(int locationId, int speciesId, bool follows)
        {
            if (!await _unitOfWork.Locations.Any(l => l.Id == locationId))
            {
                throw new KeyNotFoundException($"Location with id {locationId} not found.");
            }
            if (!await _unitOfWork.Species.Any(s => s.Id == speciesId))
            {
                throw new KeyNotFoundException($"Species with id {speciesId} not found.");
            }

            var existing = await _unitOfWork.LocationSpeciesFollowsRegions.Find(
                f => f.LocationId == locationId && f.SpeciesId == speciesId);

            if (!follows)
            {
                if (existing != null)
                {
                    _unitOfWork.LocationSpeciesFollowsRegions.Delete(existing);
                    await _unitOfWork.SaveChanges();
                }
                return;
            }

            // Already following. Not an error — the unique index makes a second row
            // impossible anyway, and a repeated click should be a no-op, not a 500.
            if (existing != null)
            {
                return;
            }

            var ownRule = await _unitOfWork.SpeciesRegulations.Find(
                r => r.SpeciesId == speciesId && r.Locations.Any(l => l.Id == locationId));
            if (ownRule != null)
            {
                throw new ArgumentException(
                    "This water has its own rule for that species. Remove it before following the region.");
            }

            _unitOfWork.LocationSpeciesFollowsRegions.Add(new LocationSpeciesFollowsRegion
            {
                LocationId = locationId,
                SpeciesId = speciesId,
                Created = DateTime.UtcNow
            });
            await _unitOfWork.SaveChanges();
        }

        /// <summary>
        /// Resolves one species at one water into one rule per fin state that actually
        /// behaves differently there.
        ///
        /// Every fin state is resolved independently against the rules that could apply to
        /// it — its own plus the unqualified ones. When they all land on the same regulation
        /// the species does not distinguish fin states here, so they collapse into a single
        /// unlabelled rule; that is the case for every species with no variant rules, which
        /// is why this returns exactly what it did before variants existed.
        /// </summary>
        /// <param name="speciesGroup">Every candidate regulation for one species.</param>
        /// <param name="locationId">The water being resolved.</param>
        /// <param name="rank">Region specificity of a candidate, ascending.</param>
        private List<LocationSpeciesRuleDTO> ResolveSpecies(
            IEnumerable<SpeciesRegulation> speciesGroup,
            int locationId,
            Func<SpeciesRegulation, int> rank)
        {
            var resolved = Enum.GetValues<AdiposeFin>()
                .Select(fin => (
                    Fin: fin,
                    // Region specificity stays the primary axis: a rule set closer to the
                    // water wins even when it names no fin state. The fin only breaks a tie
                    // between candidates of equal rank, where the exact match wins.
                    Ordered: speciesGroup
                        .Where(r => r.AdiposeFin == null || r.AdiposeFin == fin)
                        .OrderBy(rank)
                        .ThenBy(r => r.AdiposeFin == null ? 1 : 0)
                        .ToList()))
                .Where(x => x.Ordered.Count > 0)
                .ToList();

            if (resolved.Count == 0)
            {
                return [];
            }

            var winners = resolved.Select(x => x.Ordered[0].Id).Distinct().Count();
            if (winners == 1)
            {
                // No fin state is treated differently, so labelling the rule with one would
                // imply a distinction the regulations don't make.
                return [ToRule(resolved[0].Ordered, locationId, fin: null)];
            }

            return resolved.Select(x => ToRule(x.Ordered, locationId, x.Fin)).ToList();
        }

        /// <summary>
        /// Turns an ordered candidate list into the rule that applies, carrying the
        /// runner-up as the fallback.
        /// </summary>
        /// <param name="ordered">Candidates, most specific first.</param>
        /// <param name="locationId">The water being resolved.</param>
        /// <param name="fin">
        /// The fin state this rule covers, which is not necessarily the winning
        /// regulation's own value — an unqualified rule can be what applies to clipped fish
        /// once an intact-only rule has taken the other half of the species.
        /// </param>
        private LocationSpeciesRuleDTO ToRule(List<SpeciesRegulation> ordered, int locationId, AdiposeFin? fin)
        {
            // The winner is the rule that applies; the runner-up is what a maintainer would
            // fall back to if the winner were removed, which the editor needs in order to
            // say so.
            var dto = ToRule(ordered[0], locationId, fin);
            dto.FallsBackTo = ordered.Count > 1 ? ToRule(ordered[1], locationId, fin) : null;
            return dto;
        }

        private LocationSpeciesRuleDTO ToRule(SpeciesRegulation regulation, int locationId, AdiposeFin? fin)
        {
            var dto = _mapper.Map<LocationSpeciesRuleDTO>(regulation);
            dto.Source = ResolveSource(regulation, locationId);
            dto.AdiposeFin = fin;
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
                // Variants of one scope sit together, the unqualified rule first.
                .ThenBy(r => r.AdiposeFin == null ? 0 : (int)r.AdiposeFin + 1)
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
                // "National", not "Root": this string is the user-facing label the
                // client renders on a rule badge, not the tier's name. The tier was
                // renamed because a top-level region needn't be a country; what to
                // call it to an angler is a separate question, still open in
                // robwes/fishingmap.web#16. Changing it here is a wire change.
                return r.Region.Type == RegionType.Root
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

            var nationalId = await _unitOfWork.Regions.GetRootRegionId();
            return new List<int> { nationalId };
        }

        // Days per month for validating protected periods. February is 29:
        // periods carry no year, so "end of February" has to be expressible,
        // and periods are compared as month/day ordinals rather than as dates.
        private static readonly int[] DaysInMonth = [31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];

        /// <summary>
        /// Rejects a period naming a day its month doesn't have. The per-property
        /// [Range(1, 31)] on ProtectedPeriodDTO passes 31 February quite happily.
        ///
        /// A start after its end is NOT invalid — that is a period wrapping past
        /// new year, which is how most winter closures are expressed.
        /// </summary>
        private static void ValidateProtectedPeriods(IEnumerable<ProtectedPeriodDTO> periods)
        {
            foreach (var period in periods ?? [])
            {
                ValidateDayOfMonth(period.StartMonth, period.StartDay, "start");
                ValidateDayOfMonth(period.EndMonth, period.EndDay, "end");
            }
        }

        private static void ValidateDayOfMonth(int month, int day, string which)
        {
            if (month < 1 || month > 12)
            {
                throw new ArgumentException($"Protected period {which} month must be between 1 and 12.");
            }

            var lastDay = DaysInMonth[month - 1];
            if (day < 1 || day > lastDay)
            {
                throw new ArgumentException(
                    $"Protected period {which} day {day} is not a valid day of month {month} (1-{lastDay}).");
            }
        }

        private async Task ValidateScope(int? regionId, IEnumerable<int> locationIds, int? currentRegulationId, int speciesId, AdiposeFin? adiposeFin)
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
                throw new ArgumentException("A regulation must specify either a RegionId (use the root region for rules that apply everywhere) or one or more LocationIds.");
            }

            if (hasRegion && !await _unitOfWork.Regions.Any(r => r.Id == regionId!.Value))
            {
                throw new ArgumentException($"Region with id {regionId!.Value} not found.");
            }

            if (hasLocations)
            {
                // Scoped to the fin state as well as the species: trout with an intact fin and
                // trout with a clipped one are two legitimate rules at the same water, and
                // without this they would collide as a duplicate.
                var conflict = await _unitOfWork.SpeciesRegulations.Find(
                    r => r.Id != (currentRegulationId ?? 0)
                         && r.SpeciesId == speciesId
                         && r.AdiposeFin == adiposeFin
                         && r.Locations.Any(l => locationIdList.Contains(l.Id)));
                if (conflict != null)
                {
                    throw new ArgumentException("One or more locations already have a location-scoped rule for this species and adipose fin state.");
                }
            }
        }

        /// <summary>
        /// Drops the "follows the region" decision at every water a custom rule now covers.
        ///
        /// The two states are exclusive, and writing a rule for a water is itself the
        /// decision to stop inheriting there. Leaving the row behind would make a species
        /// both follow and override, and switching back later would silently resurrect an
        /// inheritance nobody re-chose.
        /// </summary>
        /// <param name="speciesId">The species the rule covers.</param>
        /// <param name="locationIds">The waters it is scoped to.</param>
        private async Task ClearFollowsRegion(int speciesId, IEnumerable<int> locationIds)
        {
            var ids = locationIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
            {
                return;
            }

            var rows = await _unitOfWork.LocationSpeciesFollowsRegions.GetAll(
                f => f.SpeciesId == speciesId && ids.Contains(f.LocationId));

            foreach (var row in rows)
            {
                _unitOfWork.LocationSpeciesFollowsRegions.Delete(row);
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
