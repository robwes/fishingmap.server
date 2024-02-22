namespace FishingMap.Domain.DTO.Images
{
    public class ImageDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? AuthorLink { get; set; }
    }
}
