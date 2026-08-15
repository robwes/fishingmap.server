namespace FishingMap.Domain.DTO.Regulations
{
    /// <summary>
    /// Sets whether a water inherits its region's rules for one species.
    ///
    /// A body rather than two verbs so the two transitions are one operation with one
    /// meaning: false does not mean "delete a rule", it means the species goes back to
    /// having no decision recorded at this water.
    /// </summary>
    public class FollowsRegionUpdate
    {
        public bool Follows { get; set; }
    }
}
