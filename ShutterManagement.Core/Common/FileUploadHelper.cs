using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.Common
{
    public class FileUploadHelper
    {
        private readonly IHostEnvironment environment;
        private readonly IConfiguration configuration;

        public FileUploadHelper(IHostEnvironment environment,IConfiguration configuration)
        {
            this.environment = environment;
            this.configuration = configuration;
        }

        public async Task<IResponseVm<string>> UploadFile(IFormFile file, string tenantId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                if (file == null || file.Length == 0)
                {
                    responseVm.Message = "Please add atleast One file.";
                    return responseVm;
                }
                var fileext = Path.GetExtension(file.FileName).ToLower();


                string[] ExAllow = [".jpg", ".png", ".jpeg", ".doc", ".docx", ".pdf"];
                if (!ExAllow.Contains(fileext))
                {
                    responseVm.Message = "accpect only Extensions: " + ExAllow.ToString();
                    return responseVm;
                }
                var rootPath = Path.Combine(AppConstant.FileUploadPath, tenantId);

                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }

                var fileName = Guid.NewGuid().ToString() + "_" + file.FileName.Replace(" ", "");
                var tenantPath = Path.Combine(tenantId, fileName);
                var savePath = Path.Combine(rootPath, fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UploadSuccessMsg;
                responseVm.Data = Path.Combine(AppConstant.FileVirtualPath, tenantPath);
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
    }
}

