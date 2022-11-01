using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Interfaces
{
    public interface IFileService
    {
        Task<string> AddImage(IFormFile image, string folder);
        void DeleteImage(string path);
    }
}
