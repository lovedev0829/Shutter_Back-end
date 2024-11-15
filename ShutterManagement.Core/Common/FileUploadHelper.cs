//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Hosting;
//using ShutterManagement.Core.ViewModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace ShutterManagement.Core.Common
//{
//    public class FileUploadHelper
//    {
//        private readonly IHostEnvironment environment;

//        public FileUploadHelper(IHostEnvironment environment)
//        {
//            this.environment = environment;
//        }

//        public async Task<IResponseVm<string>> UploadFile(IFormFile file, FileType fileType, string tenantId)
//        {
//            IResponseVm<string> responseVm = new ResponseVm<string>();
//            if (file == null || file.Length == 0)
//            {
//                responseVm.Message = "Please add atleast One file.";
//                return responseVm;
//            }
//            var fileext = Path.GetExtension(file.FileName).ToLower();

//            if (fileType == FileType.Image)
//            {
//                string[] ImageExAllow = ["jpg", "png", "jpeg"];
//                if (!ImageExAllow.Contains(fileext))
//                {
//                    responseVm.Message = "Images accpect only Extensions: " + ImageExAllow.ToString();
//                    return responseVm;
//                }
//            }
//            if (fileType == FileType.Document)
//            {
//                string[] DocExAllow = ["doc", "docx"];
//                if (!DocExAllow.Contains(fileext))
//                {
//                    responseVm.Message = "Document accpect only Extensions: " + DocExAllow.ToString();
//                    return responseVm;
//                }
//            }
//            var tenantFolderPath = Path.Combine(environment.ContentRootPath, "Files", tenantId);

//            if (!Directory.Exists(tenantFolderPath))
//            {
//                Directory.CreateDirectory(tenantFolderPath);
//            }

//            var fileName = Guid.NewGuid().ToString() + fileext;
//            var savePath = Path.Combine(tenantFolderPath, fileName);

//            using (var stream = new FileStream(savePath, FileMode.Create))
//            {
//                await file.CopyToAsync(stream)
//;
//            }

//            return fileName;
//        }
//    }
//}

