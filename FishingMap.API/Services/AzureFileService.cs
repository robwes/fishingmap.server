using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using FishingMap.Domain.Interfaces;

namespace FishingMap.API.Services
{
    public class AzureFileService : IFileService
    {
        private readonly ShareClient _shareClient;

        public AzureFileService(IFishingMapConfiguration config) 
        {
            _shareClient = new ShareClient(
                config.FileShareConnectionString,
                config.FileShareName
                );
        }
        public async Task<string> AddFile(IFormFile file, string folderPath)
        {
            var subFolderNames = folderPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            // Create the sub-folders if they don't already exist
            ShareDirectoryClient directoryClient = _shareClient.GetRootDirectoryClient();
            foreach (string subFolderName in subFolderNames)
            {
                directoryClient = directoryClient.GetSubdirectoryClient(subFolderName);
                await directoryClient.CreateIfNotExistsAsync();
            }

            // Generate a unique file name for the uploaded file
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // Upload the file to the final sub-folder in ranges of at most 4 MiB
            // (the Azure Files per-range limit)
            const int maxRangeSize = 4 * 1024 * 1024;
            ShareFileClient fileClient = directoryClient.GetFileClient(uniqueFileName);
            using (Stream stream = file.OpenReadStream())
            {
                await fileClient.CreateAsync(file.Length);

                var buffer = new byte[maxRangeSize];
                long offset = 0;
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    using var rangeStream = new MemoryStream(buffer, 0, bytesRead, writable: false);
                    await fileClient.UploadRangeAsync(new HttpRange(offset, bytesRead), rangeStream);
                    offset += bytesRead;
                }
            }

            // Return the file path relative to the root directory of the file share
            return string.Join("/", subFolderNames) + "/" + uniqueFileName;
        }

        public async Task<ContentTypeStream?> GetFile(string filePath)
        {
            // Get the file from the file share
            var directoryClient = _shareClient.GetRootDirectoryClient();

            var subDirectories = filePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            foreach (string subDirectory in subDirectories.Take(subDirectories.Length - 1))
            {
                directoryClient = directoryClient.GetSubdirectoryClient(subDirectory);
                if (!(await directoryClient.ExistsAsync()))
                {
                    return null;
                }
            }

            string fileName = subDirectories.Last();
            ShareFileClient fileClient = directoryClient.GetFileClient(fileName);
            if (!(await fileClient.ExistsAsync())) 
            {
                return null;
            }

            ShareFileProperties properties = await fileClient.GetPropertiesAsync();
            ShareFileDownloadInfo download = await fileClient.DownloadAsync();

            // Return the file as a ContentTypeStream with the appropriate content type
            return new ContentTypeStream(download.Content, properties.ContentType);
        }

        public async Task DeleteFile(string filePath)
        {
            // Extract the sub-folder names from the file path
            var subFolderNames = filePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // Get the directory client for the sub-folders
            ShareDirectoryClient directoryClient = _shareClient.GetRootDirectoryClient();
            foreach (string subFolderName in subFolderNames.Take(subFolderNames.Length - 1))
            {
                directoryClient = directoryClient.GetSubdirectoryClient(subFolderName);
                if (!(await directoryClient.ExistsAsync()))
                {
                    return;
                }
            }

            // Get the file client and delete the file
            ShareFileClient fileClient = directoryClient.GetFileClient(subFolderNames.Last());
            await fileClient.DeleteIfExistsAsync();
        }

        public async Task DeleteFolder(string folderPath)
        {
            var directoryClient = _shareClient.GetDirectoryClient(folderPath);

            if (!await directoryClient.ExistsAsync())
            {
                return;
            }

            await foreach (var fileItem in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (fileItem.IsDirectory)
                {
                    // fileItem.Name is bare; the recursive call resolves from the share root
                    await DeleteFolder($"{folderPath}/{fileItem.Name}");
                }
                else
                {
                    var fileClient = directoryClient.GetFileClient(fileItem.Name);
                    await fileClient.DeleteAsync();
                }
            }

            await directoryClient.DeleteAsync();
        }
    }
}
