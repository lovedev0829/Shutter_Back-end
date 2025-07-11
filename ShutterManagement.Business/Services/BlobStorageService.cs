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
        private readonly ClaimsProperty claimsProperty;

        public BlobStorageService(ClaimsProperty claimsProperty)
        {
            _containerClient = new BlobContainerClient(AppConstant.AzureBlobStorage_ConnectionString, AppConstant.AzureBlobStorage_ContainerName);
            _containerClient.CreateIfNotExists();
            this.claimsProperty = claimsProperty;
        }

        public async Task<IResponseVm<string>> UploadAsync(IFormFile? file, string FileTitle, Guid? Jobid, string? oldBlobUrl = null)
        {
            var response = new ResponseVm<string>();

            try
            {
                // 1. Validate file presence
                if (file == null || file.Length == 0)
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid file.";
                    return response;
                }

                // 2. Validate file extension
                var allowedExtensions = new[] { ".pdf", ".xls", ".xlsx", ".doc", ".docx", ".jpeg", ".jpg", ".png" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    response.IsSuccess = false;
                    response.Message = $"File type {extension} is not allowed.";
                    return response;
                }

                // 3. Validate file size (<= 25MB)
                const long maxSizeInBytes = 25 * 1024 * 1024;
                if (file.Length > maxSizeInBytes)
                {
                    response.IsSuccess = false;
                    response.Message = "File size exceeds the 25MB limit.";
                    return response;
                }

                // 4. Delete old blob if provided
                if (!string.IsNullOrEmpty(oldBlobUrl))
                {
                    var oldBlobName = Path.GetFileName(new Uri(oldBlobUrl).AbsolutePath);
                    var oldBlobClient = _containerClient.GetBlobClient(oldBlobName);
                    await oldBlobClient.DeleteIfExistsAsync();
                }

                // 5. Construct blob name with folder using tenantId/companyCode
                var tenantId = claimsProperty.GetTenantId().ToString();
                var companyCode = claimsProperty.GetCompanyCode();
                var folderPath = $"{tenantId}_{companyCode}/{Jobid}";
                var uniqueFileName = $"{FileTitle}_{Guid.NewGuid()}{extension}";
                var blobName = $"{folderPath}/{uniqueFileName}";

                var blobClient = _containerClient.GetBlobClient(blobName);

                // 6. Upload blob
                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, overwrite: true);

                // 7. Success response
                response.IsSuccess = true;
                response.Message = AppConstant.UploadSuccessMsg;
                response.Data = blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Upload failed: " + ex.Message;
            }

            return response;
        }

        public async Task<IResponseVm<string>> DownloadBlobAsBase64AndSaveAsync(string blobUrl)
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

                var uri = new Uri(blobUrl);
                string blobName = uri.AbsolutePath.TrimStart('/');
                if (blobName.StartsWith(AppConstant.AzureBlobStorage_ContainerName + "/"))
                {
                    blobName = blobName.Substring(AppConstant.AzureBlobStorage_ContainerName.Length + 1);
                }

                string fileName = Path.GetFileName(blobName);

                var blobClient = _containerClient.GetBlobClient(blobName);

                if (!await blobClient.ExistsAsync())
                {
                    response.IsSuccess = false;
                    response.Message = "Blob not found.";
                    return response;
                }

                using var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();
                string base64 = Convert.ToBase64String(fileBytes);
                string localFolder = Path.Combine(Directory.GetCurrentDirectory(), "DownloadedFiles");
                Directory.CreateDirectory(localFolder);
                string localFilePath = Path.Combine(localFolder, fileName);
                await File.WriteAllBytesAsync(localFilePath, fileBytes);

                 File.Delete(localFilePath);

                response.IsSuccess = true;
                response.Message = "Blob downloaded and saved successfully.";
                response.Data = base64;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Error: " + ex.Message;
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
