namespace FashionHubWeb.Helpers
{
    public class MyTool
    {
        public static async Task<string> UploadFileToFolder(IFormFile file, string folderName = "products")
        {
            try
            {
                // Generate unique filename to avoid collisions
                var extension = Path.GetExtension(file.FileName);
                var uniqueName = $"{Guid.NewGuid():N}{extension}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folderName);

                // Create folder if not exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, uniqueName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return uniqueName;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static async Task<List<string>> UploadMultipleFiles(List<IFormFile> files, string folderName = "products")
        {
            var results = new List<string>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = await UploadFileToFolder(file, folderName);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        results.Add(fileName);
                    }
                }
            }
            return results;
        }

        public static void DeleteFileFromFolder(string fileName, string folderName = "products")
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folderName, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Silently ignore delete errors
            }
        }
    }
}
