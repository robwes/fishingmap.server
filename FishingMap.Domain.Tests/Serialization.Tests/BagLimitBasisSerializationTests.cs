using FishingMap.Data.Entities;
using FishingMap.Domain.DTO.Regulations;
using System.Text.Json;

namespace FishingMap.Domain.Tests.Serialization.Tests
{
    /// <summary>
    /// Locks the wire contract for <see cref="BagLimitBasis"/>, the same way
    /// RegionTypeSerializationTests does for RegionType. The frontend maps these names to
    /// "per day" / "per permit" and shows a bare count when the value is null, so a change
    /// in shape here would silently misdescribe a legal limit.
    /// </summary>
    public class BagLimitBasisSerializationTests
    {
        private static readonly JsonSerializerOptions WebOptions = new(JsonSerializerDefaults.Web);

        [Theory]
        [InlineData(BagLimitBasis.Day, "Day")]
        [InlineData(BagLimitBasis.Week, "Week")]
        [InlineData(BagLimitBasis.Season, "Season")]
        [InlineData(BagLimitBasis.Year, "Year")]
        [InlineData(BagLimitBasis.Permit, "Permit")]
        public void SerializesBasisAsItsName(BagLimitBasis basis, string expected)
        {
            var json = JsonSerializer.Serialize(
                new SpeciesRegulationDTO { Id = 1, SpeciesId = 1, BagLimit = 4, BagLimitBasis = basis }, WebOptions);

            using var doc = JsonDocument.Parse(json);
            var value = doc.RootElement.GetProperty("bagLimitBasis");

            Assert.Equal(JsonValueKind.String, value.ValueKind);
            Assert.Equal(expected, value.GetString());
        }

        [Fact]
        public void SerializesUnsetBasisAsNull()
        {
            // Null is meaningful: the regulation doesn't say what the limit is counted
            // against, and the UI must show a bare count rather than assume "per day".
            var json = JsonSerializer.Serialize(
                new SpeciesRegulationDTO { Id = 1, SpeciesId = 1, BagLimit = 4 }, WebOptions);

            using var doc = JsonDocument.Parse(json);

            Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("bagLimitBasis").ValueKind);
        }

        [Fact]
        public void DeserializesBasisFromName()
        {
            var add = JsonSerializer.Deserialize<SpeciesRegulationAdd>(
                """{"speciesId":1,"regionId":1,"bagLimit":4,"bagLimitBasis":"Permit"}""", WebOptions);

            Assert.NotNull(add);
            Assert.Equal(BagLimitBasis.Permit, add!.BagLimitBasis);
        }

        [Fact]
        public void DeserializesMissingBasisAsNull()
        {
            var add = JsonSerializer.Deserialize<SpeciesRegulationAdd>(
                """{"speciesId":1,"regionId":1,"bagLimit":4}""", WebOptions);

            Assert.NotNull(add);
            Assert.Null(add!.BagLimitBasis);
        }
    }
}
