using System.Text.Json.Serialization;

namespace FishingMap.Data.Entities
{
    // Serialized as its name ("Ely") rather than its number, so that inserting
    // a new tier into the hierarchy can't silently shift existing values and
    // leave clients mislabelling every region. Deserialization still accepts
    // integers (allowIntegerValues defaults to true), so request bodies
    // sending 0|1|2 keep working.
    // Persisted as an int in the Regions table — renumbering these members is
    // still a data migration.
    [JsonConverter(typeof(JsonStringEnumConverter<RegionType>))]
    public enum RegionType
    {
        // The top of the hierarchy — today the row named "Finland", but the tier is
        // Root rather than National because a top-level region need not be a country.
        // The rule Source string still reads "National"; that is a display label, not
        // this name. See robwes/fishingmap.web#16.
        Root = 0,
        Ely = 1,
        ManagementArea = 2
    }
}
