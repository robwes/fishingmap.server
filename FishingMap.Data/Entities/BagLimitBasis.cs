using System.Text.Json.Serialization;

namespace FishingMap.Data.Entities
{
    // What a bag limit is counted against. Deliberately NOT a duration: put-and-take waters
    // sell a permit covering a fixed number of fish, so "per permit" sits alongside the
    // time-based bases. A null BagLimitBasis on a regulation means the source rule does not
    // say — clients must show the bare count rather than assume a daily limit.
    //
    // Serialized by name for the same reason as RegionType: inserting a member would
    // otherwise silently re-label every existing row in every client.
    [JsonConverter(typeof(JsonStringEnumConverter<BagLimitBasis>))]
    public enum BagLimitBasis
    {
        Day = 0,
        Week = 1,
        Season = 2,
        Year = 3,
        Permit = 4
    }
}
