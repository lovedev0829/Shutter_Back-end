using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService()
        {
            _containerClient = new BlobContainerClient(AppConstant.AzureBlobStorage_ConnectionString, AppConstant.AzureBlobStorage_ContainerName);
            _containerClient.CreateIfNotExists();
        }

        public async Task<IResponseVm<string>> UploadAsync(IFormFile? file)
        {
            var response = new ResponseVm<string>();
            try
            {
                if (file == null || file.Length == 0)
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid file.";
                    return response;
                }

                var blobName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var blobClient = _containerClient.GetBlobClient(blobName);

                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, overwrite: true);

                response.IsSuccess = true;
                response.Message = AppConstant.UploadSuccessMsg;
                response.Data= blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Upload failed: " + ex.Message;
            }

            return response;
        }

        public async Task<IResponseVm<string>> DeleteAsync(string blobUrl)
        {
            var response = new ResponseVm<string>();
            try
            {
                if (string.IsNullOrEmpty(blobUrl))
                {
                    response.IsSuccess = false;
                    response.Message = "Blob URL is required.";
                    return response;
                }

                var blobName = Path.GetFileName(new Uri(blobUrl).AbsolutePath);
                var blobClient = _containerClient.GetBlobClient(blobName);
                var result = await blobClient.DeleteIfExistsAsync();
                response.IsSuccess = true;
                response.Message = result ? "File deleted successfully." : "File not found.";
            }
            catch (Exception ex)
            {
                response.Message = "Deletion failed: " + ex.Message;
            }

            return response;
        }
    }

}
