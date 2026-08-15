using FishingMap.Data.Interfaces;

namespace FishingMap.Data.Entities
{
    /// <summary>
    /// Records that an administrator chose to let a water inherit its region's rules for one
    /// species. The row existing IS the decision — there is no "mode" column, because the
    /// other two states already have exactly one representation each: a location-scoped
    /// SpeciesRegulation means a custom rule, and neither means nobody has decided yet.
    ///
    /// Nothing is inherited without a row here. Absence used to mean "inherit"; it now means
    /// "not looked at", and the UI must say so rather than showing an empty rule list, which
    /// would read as "no restrictions" on a species that may well be protected.
    ///
    /// It deliberately stores NO region id. Location.RegionId already says where the water
    /// sits, and following means following whatever the cascade yields at read time — so a
    /// nearer region writing a rule, or the water being moved, both take effect on their own.
    /// A region id here would be a second answer to a question that already has one, and it
    /// would point at the old region after a move.
    ///
    /// Granularity is per species, not per adipose fin state: following gets every rule the
    /// region has for the species, variants included.
    ///
    /// See decision 11 in robwes/fishingmap.web#13.
    /// </summary>
    public class LocationSpeciesFollowsRegion : IEntity
    {
        // A surrogate key rather than the natural (LocationId, SpeciesId) composite, because
        // IRepository<TEntity> requires IEntity and IEntity requires an int Id. The unique
        // index in ApplicationDbContext enforces the real key.
        public int Id { get; set; }

        public int LocationId { get; set; }
        public virtual Location Location { get; set; } = null!;

        public int SpeciesId { get; set; }
        public virtual Species Species { get; set; } = null!;

        public DateTime Created { get; set; }
    }
}
