using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using ShutterManagement.Core;
using System.Reflection.Metadata;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels.Response;
using System.Linq.Dynamic.Core;

namespace ShutterManagement.Business.Services
{


    public class UploadBlobDocumentService : BaseRepository<ShutterDBContext, TblBlobdocument>, IUploadBlobDocument
    {
        private readonly BlobStorageService _blobService;
        private readonly ClaimsProperty claimsProperty;
        private readonly TenantServices tenantServices;

        public UploadBlobDocumentService(BlobStorageService blobService ,ClaimsProperty claimsProperty,TenantServices tenantServices)
        {
            _blobService = blobService;
            this.claimsProperty = claimsProperty;
            this.tenantServices = tenantServices;
        }

        public async Task<IResponseVm<string>> UploadDocument(CreateBlobDocument model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var isTenantValid = await tenantServices.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var uploadeToBlobRes = await _blobService.UploadAsync(model.BlobDocument);

                if(uploadeToBlobRes == null && uploadeToBlobRes?.Data==null)
                {
                    responseVm.Message = "Failed to upload document. Please try again.";
                    return responseVm;
                }
                var document = new TblBlobdocument
                {
                    DocumentId = Guid.NewGuid(),
                    Title = model.DocumentTitle,
                    TenantId = claimsProperty.GetTenantId(),
                    CreatedBy = claimsProperty.GetUserId(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true,
                    IsDelete = false,
                    BlobUrl = uploadeToBlobRes.Data ?? ""
                };
                await Add(document);
                responseVm.Message = $"Document {AppConstant.UploadSuccessMsg}";
                responseVm.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> UpdateDocument(UpdateBlobDocument model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var isTenantValid = await tenantServices.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var ExistDocumentUploaded = GetAllQuerable().Where(x => x.DocumentId == model.DocumentId && x.IsDelete == false).FirstOrDefault();

                if (ExistDocumentUploaded == null)
                {
                    responseVm.Message = "Document not found.";
                }
                if (ExistDocumentUploaded.BlobUrl != null)
                {
                    await _blobService.DeleteAsync(ExistDocumentUploaded.BlobUrl);
                    var BlobDataAsync = await _blobService.UploadAsync(model.BlobDocument);
                    model.BlobUrl= BlobDataAsync.Data ?? "";

                }

                ExistDocumentUploaded.Title = model.DocumentTitle;
                ExistDocumentUploaded.BlobUrl = model.BlobUrl;
                ExistDocumentUploaded.ModifiedBy = claimsProperty.GetUserId();
                ExistDocumentUploaded.TenantId = claimsProperty.GetTenantId();
                ExistDocumentUploaded.UpdatedAt = DateTime.Now;

                await Update(ExistDocumentUploaded);

                responseVm.Message = $"Document {AppConstant.UploadSuccessMsg}";
                responseVm.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> DeleteDocument(Guid? DocumentId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var isTenantValid = await tenantServices.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.Message = $"Document {AppConstant.TenantNotFound}";
                    return responseVm;
                }
                var ExistDocumentUploaded = GetAllQuerable().Where(x => x.DocumentId == DocumentId && x.IsDelete==false).FirstOrDefault();

                if (ExistDocumentUploaded == null)
                {
                    responseVm.Message = "Document not found.";
                }
                ExistDocumentUploaded.IsDelete = true;
                ExistDocumentUploaded.ModifiedBy = claimsProperty.GetUserId();
                ExistDocumentUploaded.ModifiedBy = claimsProperty.GetTenantId();
                ExistDocumentUploaded.UpdatedAt = DateTime.Now;

                await Update(ExistDocumentUploaded);

                responseVm.Message = $"Document {AppConstant.UploadSuccessMsg}";
                responseVm.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<List<GetAllBlobDocument>>> GetAllDocument(Pagination filter)
        {
            IResponseVm<List<GetAllBlobDocument>> responseVm = new ResponseVm<List<GetAllBlobDocument>>();

            try
            {
                var isTenantValid = await tenantServices.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var BlobDocumentlis = GetAllQuerable().Where(x => x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId()).OrderByDescending(x => x.CreatedAt).
                                    Select(i => new GetAllBlobDocument
                                    {
                                        DocumentId = i.DocumentId,
                                        DocumentTitle = i.Title,
                                        BlobUrl = i.BlobUrl,
                                        CreatedAt = i.CreatedAt,
                                        CreatedBy = dbContext.TblUsers.Where(x=>x.Id==i.CreatedBy).Select(s=>s.FirstName+" "+s.LastName).FirstOrDefault(),
                                    }).ToList();

                filter.SearchValue = filter?.SearchValue.Trim().ToLower(); 
                var queryable = BlobDocumentlis.AsQueryable();
                queryable = Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                queryable = Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);
                var TakenRecords = Paginate.Pagination(queryable.ToList(), filter);

                responseVm.IsSuccess = true;
                responseVm.Data = TakenRecords;
                responseVm.TotalRecords = BlobDocumentlis.Count();
                responseVm.Message = AppConstant.FetchSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }
    }

}
