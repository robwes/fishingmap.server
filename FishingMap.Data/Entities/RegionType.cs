using System.Text.Json.Serialization;

namespace FishingMap.Data.Entities
{
    // Serialized as its name ("Root") rather than its number, so that inserting
    // a new tier into the hierarchy can't silently shift existing values and
    // leave clients mislabelling every region. Deserialization still accepts
    // integers (allowIntegerValues defaults to true), so request bodies
    // sending 0|1|2 keep working.
    // Persisted as an int in the Regions table — renumbering these members is
    // still a data migration.
    [JsonConverter(typeof(JsonStringEnumConverter<RegionType>))]
    public enum RegionType
    {
        // These name a tier's POSITION in the hierarchy, never the body that currently
        // occupies it. Both exceptions to that have already gone stale: "National" assumed
        // the top is a country, and "Ely" named an organisation that Finland reorganised out
        // of existence on 1 January 2026, when the 15 ELY centres became 10 Economic
        // Development Centres. Whoever holds a tier is data — it belongs in Region.Name,
        // where a rebrand is a row edit rather than a wire change.

        // The top. Today the row named "Finland", but a top-level region need not be a
        // country. The rule Source string still reads "National"; that is a display label,
        // not this name. See robwes/fishingmap.web#16.
        Root = 0,

        // The state's regional authority for fisheries — the Economic Development Centres
        // since 2026, the ELY centres before them.
        StateRegion = 1,

        // The kalatalousalue, whose own English name is "fisheries region". Named for the
        // thing itself rather than positionally, unlike the two above, because this is not a
        // position an organisation occupies — it IS the statutory category, so there is
        // nothing to be reorganised out from under the name. It was called ManagementArea
        // until 2026-08-16; that appears to have come from the käyttö- ja hoitosuunnitelma,
        // the "use and management plan", where the word "management" actually belongs.
        FisheriesRegion = 2
    }
}
