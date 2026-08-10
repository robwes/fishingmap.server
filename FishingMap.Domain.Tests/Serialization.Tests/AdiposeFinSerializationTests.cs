using FishingMap.Data.Entities;
using FishingMap.Domain.DTO.Regulations;
using System.Text.Json;

namespace FishingMap.Domain.Tests.Serialization.Tests
{
    /// <summary>
    /// Locks the wire contract for <see cref="AdiposeFin"/>, as
    /// RegionTypeSerializationTests and BagLimitBasisSerializationTests do for theirs.
    ///
    /// Null carries meaning here beyond "unset": the rule applies whatever the fin looks
    /// like. Serializing it as anything else would narrow every rule written before variants
    /// existed to one half of its species.
    /// </summary>
    public class AdiposeFinSerializationTests
    {
        private static readonly JsonSerializerOptions WebOptions = new(JsonSerializerDefaults.Web);

        [Theory]
        [InlineData(AdiposeFin.Intact, "Intact")]
        [InlineData(AdiposeFin.Clipped, "Clipped")]
        public void SerializesFinAsItsName(AdiposeFin fin, string expected)
        {
            var json = JsonSerializer.Serialize(
                new SpeciesRegulationDTO { Id = 1, SpeciesId = 1, AdiposeFin = fin }, WebOptions);

            using var doc = JsonDocument.Parse(json);
            var value = doc.RootElement.GetProperty("adiposeFin");

            Assert.Equal(JsonValueKind.String, value.ValueKind);
            Assert.Equal(expected, value.GetString());
        }

        [Fact]
        public void SerializesAnUnnarrowedRuleAsNull()
        {
            var json = JsonSerializer.Serialize(
                new SpeciesRegulationDTO { Id = 1, SpeciesId = 1 }, WebOptions);

            using var doc = JsonDocument.Parse(json);

            Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("adiposeFin").ValueKind);
        }

        [Fact]
        public void DeserializesFinFromName()
        {
            var add = JsonSerializer.Deserialize<SpeciesRegulationAdd>(
                """{"speciesId":1,"regionId":1,"adiposeFin":"Clipped"}""", WebOptions);

            Assert.NotNull(add);
            Assert.Equal(AdiposeFin.Clipped, add!.AdiposeFin);
        }

        [Fact]
        public void DeserializesAMissingFinAsNull()
        {
            var add = JsonSerializer.Deserialize<SpeciesRegulationAdd>(
                """{"speciesId":1,"regionId":1}""", WebOptions);

            Assert.NotNull(add);
            Assert.Null(add!.AdiposeFin);
        }

        [Fact]
        public void KeepsFullProtectionSeparateFromCatchAndRelease()
        {
            // Two different claims: one says the fish is off-limits, the other says fishing
            // for it is fine as long as it goes back. Neither may imply the other.
            var add = JsonSerializer.Deserialize<SpeciesRegulationAdd>(
                """{"speciesId":1,"regionId":1,"isFullyProtected":true}""", WebOptions);

            Assert.NotNull(add);
            Assert.True(add!.IsFullyProtected);
            Assert.False(add.IsCatchAndReleaseOnly);
        }
    }
}
