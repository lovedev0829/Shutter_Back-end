//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Hosting;  
//using ShutterManagement.Business.Services;
//using System;
//using System.IO;
//using System.Threading.Tasks;

//namespace ShutterManagement.Business.Services
//{
//    public interface IFileUploadService
//    {
//        Task<string> UploadImageAsync(IFormFile file);
//    }

//    public class FileUploadService : IFileUploadService
//    {
//        private readonly IWebHostEnvironment _webHostEnvironment;
//        private readonly IHostEnvironment _hostEnvironment;

//        public FileUploadService(IWebHostEnvironment webHostEnvironment, IHostEnvironment hostEnvironment)
//        {
//            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
//            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
//        }

//        public async Task<string> UploadImageAsync(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//            {
//                throw new Exception("No file uploaded.");
//            }

//            var imgExt = Path.GetExtension(file.FileName).ToLower();
//            if (imgExt != ".jpg" && imgExt != ".png" && imgExt != ".jfif")
//            {
//                throw new Exception("Only JPG, PNG, and JFIF files are allowed.");
//            }

//            var fileName = Guid.NewGuid().ToString() + imgExt;

//            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Content", "Image");

//            if (string.IsNullOrEmpty(_webHostEnvironment.WebRootPath))
//            {
//                uploadsFolder = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "Content", "Image");
//            }

//            if (!Directory.Exists(uploadsFolder))
//            {
//                Directory.CreateDirectory(uploadsFolder);
//            }

//            var savePath = Path.Combine(uploadsFolder, fileName);

//            using (var stream = new FileStream(savePath, FileMode.Create))
//            {
//                await file.CopyToAsync(stream);
//            }

//            return fileName;  
//        }
//    }
//}
