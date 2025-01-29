using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.Common;

namespace ShutterManagement.Business.Services
{
    public class SubtenantProductMarkup : BaseRepository<BtprojecShuttermanagementDevContext, TblSubtenantProductMarkup> , ISubtenantProductMarkup 
    {
        private readonly BtprojecShuttermanagementDevContext _context;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;
        private readonly IProductService productService;

        public SubtenantProductMarkup(BtprojecShuttermanagementDevContext context, ClaimsProperty claimsProperty,ITenantService tenantService,IProductService productService)
        {
            _context = context;
            this.claimsProperty = claimsProperty;
            this.tenantService = tenantService;
            this.productService = productService;
        }

        public async Task<IResponseVm<string>> Create_SubtenatProductMarkup(CreateSubtenantProductmarkup model)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId != null && x.IsActive == true && x.IsDelete ==false);
                if (IsTenantExists)
                {
                    var isproductExists = GetAllQuerable().Where(x => x.ProductId.ToString().ToLower() == model.ProductId.ToString().ToLower() && x.Isdelete == false && x.IsActive == true).FirstOrDefault();
                    if(isproductExists != null)
                    {
                        responseVm.Message = "Product" + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    var markup = new TblSubtenantProductMarkup
                    {
                        ProductId = model.ProductId,
                        Markup = model.Markup,
                        TenantId = AppConstant.LoggedInTenantId,
                        ParentTenantId = Guid.Parse(AppConstant.LoggedInParentTenantId),
                        IsActive = true,
                        Isdelete = false,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID,
                    };

                    var res = await Add(markup);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                }
            }

            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> Update_SubtenantProductMarkup(UpdateSubtenantProductMarkup model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId != null && x.IsActive == true && x.IsDelete == false);
                if (IsTenantExists)
                {
                    var isproductExists = GetAllQuerable().Where(x => x.Id.ToString().ToLower() != model.Id.ToString().ToLower() && x.ProductId.ToString().ToLower() == model.ProductId.ToString().ToLower() && x.Isdelete == false && x.IsActive == true).FirstOrDefault();
                    if (isproductExists != null)
                    {
                        responseVm.Message = "Product" + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    var markup = GetAllQuerable().Where(x => x.Id == model.Id && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                    if(markup != null)
                    {
                        markup.ProductId = model.ProductId;
                        markup.Markup = model.Markup;
                        markup.UpdatedBy = AppConstant.LoggedInUserID;
                        markup.ModifiedAt = DateTime.Now;

                        await Update(markup);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                }
            }

            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }
        public async Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId != null && x.IsActive == true && x.IsDelete == false);
                if (IsTenantExists)
                {
                    var markup = GetAllQuerable().Where(x => x.Id == Id && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                    if (markup != null)
                    {
                        markup.IsActive = !markup.IsActive;
                        markup.UpdatedBy = AppConstant.LoggedInUserID;
                        markup.ModifiedAt = DateTime.Now;

                        await Update(markup);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                }
            }

            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }
        public async Task<IResponseVm<string>> Delete(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId != null && x.IsActive == true && x.IsDelete == false);
                if (IsTenantExists)
                {
                    var markup = GetAllQuerable().Where(x => x.Id == Id && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                    if (markup != null)
                    {
                        markup.IsActive = false;
                        markup.Isdelete = true;
                        markup.UpdatedBy = AppConstant.LoggedInUserID;
                        markup.ModifiedAt = DateTime.Now;

                        await Update(markup);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                }
            }

            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<GetAllSubtenantProductMarkup>>> GetAllSubtenantProductMarkup(Pagination filter)
        {
            IResponseVm<List<GetAllSubtenantProductMarkup>> responseVm = new ResponseVm<List<GetAllSubtenantProductMarkup>>();
            try
            {
                var isTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId
                    && x.ParentTenantId != null && x.IsActive == true && x.IsDelete == false);

                if (isTenantExists)
                {
                    var markups = (await GetAll())
                        .Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.Isdelete == false && x.IsActive == true)
                        .ToList(); 

                    var products =  productService.GetAllQuerable()
                        .ToList(); 

                    var markup = markups.Join(
                        products,
                        markups => markups.ProductId,
                        product => product.ProductId,
                        (markups, product) => new GetAllSubtenantProductMarkup
                        {
                            Id = markups.Id,
                            ProductId = product.ProductId,
                            ProductName = product.ProductName,
                            Markup = markups.Markup
                        }).ToList();
                    var filteredRecords = markup
                .Where(x =>
                (string.IsNullOrWhiteSpace(filter.SearchValue) ||
                ((x.ProductId.ToString() ?? "").Contains(filter.SearchValue)
                 || (x.ProductName ?? "").Contains(filter.SearchValue)
                 || (x.Markup.ToString() ?? "").Contains(filter.SearchValue)))
            ).ToList();
                    var paginatedrecords = Paginate.Pagination(filteredRecords, filter);
                    responseVm.IsSuccess = true;
                    responseVm.Data = paginatedrecords;
                    responseVm.TotalRecords = markup.Count;
                    responseVm.Message = "Data fetched successfully";
                }
                else
                {
                    responseVm.Message = "Tenant not found or does not meet the criteria.";
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = $"An error occurred: {ex.Message}";
            }

            return responseVm;
        }


   

    }
}
