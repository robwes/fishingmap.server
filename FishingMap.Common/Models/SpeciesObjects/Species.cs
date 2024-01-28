using FishingMap.Common.Models.ImageObjects;

namespace FishingMap.Common.Models.SpeciesObjects
{
    public class Species
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IEnumerable<Image> Images { get; set; } = new List<Image>();
    }
}
