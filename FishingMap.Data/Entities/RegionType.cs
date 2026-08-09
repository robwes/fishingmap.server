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
        National = 0,
        Ely = 1,
        ManagementArea = 2
    }
}
