namespace FishingMap.Common.Models.ImageObjects
{
    public class Image
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string AuthorLink { get; set; } = string.Empty;
    }
}
