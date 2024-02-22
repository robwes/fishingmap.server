using FishingMap.Domain.DTO.Images;

namespace FishingMap.Domain.DTO.Species
{
    public class SpeciesDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IEnumerable<ImageDTO> Images { get; set; } = new List<ImageDTO>();
    }
}
