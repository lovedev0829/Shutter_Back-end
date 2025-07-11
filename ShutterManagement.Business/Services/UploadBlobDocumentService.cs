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

        public UploadBlobDocumentService(BlobStorageService blobService ,ClaimsProperty claimsProperty)
        {
            _blobService = blobService;
            this.claimsProperty = claimsProperty;
        }

        public async Task<IResponseVm<string>> UploadDocument(CreateBlobDocument model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
               
                var uploadeToBlobRes = await _blobService.UploadAsync(model.File,model.DocumentTitle,model.JobId,null);

                if(!uploadeToBlobRes.IsSuccess)
                {
                    responseVm.Message = uploadeToBlobRes.Message;
                    return responseVm;
                }
                
                var document = new TblBlobdocument
                {
                    DocumentId = Guid.NewGuid(),
                    Title = model.DocumentTitle,
                    TenantId = claimsProperty.GetTenantId(),
                    CreatedBy = claimsProperty.GetUserId(),
                    JobId = model.JobId,
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
               
                var ExistDocumentUploaded = GetAllQuerable().Where(x => x.DocumentId == model.DocumentId && x.IsDelete == false).FirstOrDefault();

                if (ExistDocumentUploaded == null)
                {
                    responseVm.Message = "Document not found.";
                }
                if (ExistDocumentUploaded.BlobUrl != null)
                {
                    await _blobService.DeleteAsync(ExistDocumentUploaded.BlobUrl);
                    var BlobDataAsync = await _blobService.UploadAsync(model.File,model.DocumentTitle, model.JobId,model.BlobUrl);
                    model.BlobUrl = BlobDataAsync.Data ?? "";

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
               
                var ExistDocumentUploaded = GetAllQuerable().Where(x => x.DocumentId == DocumentId && x.IsDelete==false).FirstOrDefault();

                if (ExistDocumentUploaded == null)
                {
                    responseVm.Message = "Document not found.";
                }

                if (ExistDocumentUploaded.BlobUrl != null)
                {
                    await _blobService.DeleteAsync(ExistDocumentUploaded.BlobUrl);
                }

                ExistDocumentUploaded.IsDelete = true;
                ExistDocumentUploaded.IsActive = false;
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

        public async Task<IResponseVm<List<GetAllBlobDocument>>> GetAllDocument(Pagination filter)
        {
            IResponseVm<List<GetAllBlobDocument>> responseVm = new ResponseVm<List<GetAllBlobDocument>>();

            try
            {
                
                var BlobDocumentlist = GetAllQuerable().Where(x => x.IsDelete == false && filter.JobId.ToString().ToLower()== x.JobId.ToString().ToLower() && x.TenantId == claimsProperty.GetTenantId()).OrderByDescending(x => x.CreatedAt).
                                    Select(i => new GetAllBlobDocument
                                    {
                                        DocumentId = i.DocumentId,
                                        DocumentTitle = i.Title??"",
                                        BlobUrl = i.BlobUrl??"",
                                        CreatedAt = i.CreatedAt ?? null,
                                        JobId = i.JobId ?? null,
                                        TenantId = i.TenantId ?? null,
                                        CreatedBy = dbContext.TblUsers.Where(x=>x.Id==i.CreatedBy).Select(s=>s.FirstName+" "+s.LastName).FirstOrDefault()??"",
                                        JobName = dbContext.TblJobs.Where(x => x.JobId == i.JobId).Select(s => s.JobName).FirstOrDefault() ??"",
                                    }).ToList();

                filter.SearchValue = filter?.SearchValue.Trim().ToLower();
                if (!string.IsNullOrEmpty(filter?.SearchValue))
                {
                    BlobDocumentlist = BlobDocumentlist.Where(x => x.DocumentTitle.ToLower().Contains(filter.SearchValue) || 
                                                                x.JobName.ToLower().Contains(filter.SearchValue)).ToList();
                }

                
                var queryable = BlobDocumentlist.AsQueryable();
                queryable = Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                queryable = Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);
                var TakenRecords = Paginate.Pagination(queryable.ToList(), filter);

                responseVm.IsSuccess = true;
                responseVm.Data = TakenRecords;
                responseVm.TotalRecords = BlobDocumentlist.Count();
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
