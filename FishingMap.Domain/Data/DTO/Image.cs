using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Data.DTO
{
    public class Image
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Author { get; set; }
        public string AuthorLink { get; set; }
    }
}
