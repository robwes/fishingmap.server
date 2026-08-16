using FishingMap.Data.Entities;
using FishingMap.Domain.DTO.Regions;
using System.Text.Json;

namespace FishingMap.Domain.Tests.Serialization.Tests
{
    /// <summary>
    /// Locks the wire contract for <see cref="RegionType"/>. The frontend reads
    /// region.type as a string name and maps it to a display label, so dropping
    /// the JsonStringEnumConverter attribute would silently mislabel every
    /// region in the UI rather than fail loudly. These tests are the guard.
    /// </summary>
    public class RegionTypeSerializationTests
    {
        // Matches what ASP.NET Core uses for controllers.
        private static readonly JsonSerializerOptions WebOptions = new(JsonSerializerDefaults.Web);

        [Theory]
        [InlineData(RegionType.Root, "Root")]
        [InlineData(RegionType.StateRegion, "StateRegion")]
        [InlineData(RegionType.ManagementArea, "ManagementArea")]
        public void RegionDTO_SerializesTypeAsItsName(RegionType type, string expected)
        {
            var json = JsonSerializer.Serialize(new RegionDTO { Id = 1, Name = "x", Type = type }, WebOptions);

            using var doc = JsonDocument.Parse(json);
            var value = doc.RootElement.GetProperty("type");

            Assert.Equal(JsonValueKind.String, value.ValueKind);
            Assert.Equal(expected, value.GetString());
        }

        [Fact]
        public void RegionAdd_DeserializesTypeFromName()
        {
            var add = JsonSerializer.Deserialize<RegionAdd>(
                """{"name":"Uusimaa","type":"StateRegion"}""", WebOptions);

            Assert.NotNull(add);
            Assert.Equal(RegionType.StateRegion, add!.Type);
        }

        [Fact]
        public void RegionAdd_StillDeserializesTypeFromInteger()
        {
            // allowIntegerValues defaults to true, so existing callers posting
            // 0|1|2 keep working. Only responses changed shape.
            var add = JsonSerializer.Deserialize<RegionAdd>(
                """{"name":"Uusimaa","type":1}""", WebOptions);

            Assert.NotNull(add);
            Assert.Equal(RegionType.StateRegion, add!.Type);
        }

        [Fact]
        public void RegionUpdate_DeserializesTypeFromName()
        {
            var update = JsonSerializer.Deserialize<RegionUpdate>(
                """{"id":2,"name":"Uusimaa","type":"ManagementArea"}""", WebOptions);

            Assert.NotNull(update);
            Assert.Equal(RegionType.ManagementArea, update!.Type);
        }
    }
}
