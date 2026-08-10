using System.Text.Json.Serialization;

namespace FishingMap.Data.Entities
{
    // Whether a rule applies only to fish whose adipose fin is intact or only to fish whose
    // fin has been clipped. Wild trout and salmon keep the fin; hatchery-reared fish are
    // clipped before release, so the decree treats them as different fish: in Uusimaa an
    // intact-finned trout is fully protected while a clipped one may be kept at 50 cm.
    //
    // This is ONE AXIS, and the field holding it is nullable — null means the rule does not
    // care about the fin, which is every rule written before this existed. It is deliberately
    // not a combined "RuleAppliesTo { All, AdiposeFinIntact, ... }": folding an axis and its
    // value into one field is what makes a second per-fish axis (sex, origin) inexpressible
    // in combination, the same single-valued trap that ruled out putting geography on the
    // region tree. A second axis is a second nullable column. See robwes/fishingmap.web#16.
    //
    // Serialized by name, as RegionType and BagLimitBasis are.
    [JsonConverter(typeof(JsonStringEnumConverter<AdiposeFin>))]
    public enum AdiposeFin
    {
        Intact = 0,
        Clipped = 1
    }
}
