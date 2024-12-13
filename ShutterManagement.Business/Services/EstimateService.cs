using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.SPModel.SP_Responses;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.Common;
using static Azure.Core.HttpHeader;

namespace ShutterManagement.Business.Services
{
    public class EstimateService : BaseRepository<BtprojecShuttermanagementDevContext, TblEstimate>, IEstimateService
    {
        private readonly BtprojecShuttermanagementDevContext _context;
        private readonly IComponentService componentService;
        private readonly IComponentMaterial componentMaterialService;
        private readonly IWindowService windowService;
        private readonly IWindowProductService windowProductService;
        private readonly IEstimateWindowService estimateWindowService;
        private readonly IEstimateWindowProductService estimateWindowProductService;
        private readonly IEstimateWindowProductComponentService estimateWindowProductComponentService;
        private readonly IEstimateWindowProductComponentMaterialService estimateWindowProductComponentMaterialService;
        private readonly IProductService productService;
        private readonly ITenantService tenantService;
        private readonly ClaimsProperty claimsProperty;

        public EstimateService(BtprojecShuttermanagementDevContext context, 
            IComponentService componentService,
            IComponentMaterial componentMaterialService,
            IWindowService windowService,
            IWindowProductService windowProductService,
            IEstimateWindowService estimateWindowService,
            IEstimateWindowProductService estimateWindowProductService,
            IEstimateWindowProductComponentService estimateWindowProductComponentService,
            IEstimateWindowProductComponentMaterialService estimateWindowProductComponentMaterialService,
            IProductService productService,
            ITenantService tenantService,ClaimsProperty claimsProperty)
        {
            _context = context;
            this.componentService = componentService;
            this.componentMaterialService = componentMaterialService;
            this.windowService = windowService;
            this.windowProductService = windowProductService;
            this.estimateWindowService = estimateWindowService;
            this.estimateWindowProductService = estimateWindowProductService;
            this.estimateWindowProductComponentService = estimateWindowProductComponentService;
            this.estimateWindowProductComponentMaterialService = estimateWindowProductComponentMaterialService;
            this.productService = productService;
            this.tenantService = tenantService;
            this.claimsProperty = claimsProperty;
        }

        public async Task<IResponseVm<string>> Create_Estimate(CreateEstimate model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (IsTenantExists)
                {
                    var IsEstimateExists = GetAllQuerable()
                        .Where(c => c.EstimateName == model.EstimateName && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false).ToList();
                    if (IsEstimateExists.Count > 0)
                    {
                        responseVm.Message = "Estimates Name " + AppConstant.AlreadyExists;
                        return responseVm;
                    }

                    TblEstimate CreateEstimate = new TblEstimate
                    {
                        EstimateId = Guid.NewGuid(),
                        EstimateName = model.EstimateName,
                        EstimateCode = GenerateUniqueId.GenerateUniqueIds(ModuleName.Estimation, AppConstant.LoggedInCompanyCode),
                        Notes = model.Notes,
                        DiscountAmount = model.Discount,
                        GrandTotal = model.GrandTotal,
                        JobId = model.JobId,
                        TaxAmount = model.Tax,
                        TermsCondition = model.TermsCondition,
                        CreatedBy = AppConstant.LoggedInUserID,
                        CreatedAt = DateTime.Now,
                        TenantId = AppConstant.LoggedInTenantId,
                        IsActive = true,
                        IsDelete = false,
                        Status = EstimationValue.GetEnumValue(Estimation.New),
                    };
                    var res = await Add(CreateEstimate);
                    if (res.EstimateId != Guid.Empty)
                    {
                        foreach (var product in model?.WindowProductId)
                        {
                            var WindowProductData = windowProductService.GetAllQuerable().Where(x => x.WindowProductId == product && x.IsDelete == false).FirstOrDefault();
                            if (WindowProductData != null)
                            {
                                var windowData = windowService.GetAllQuerable().Where(x => x.WindowId == WindowProductData.WindowId && x.IsDelete == false).FirstOrDefault();
                                if(windowData == null)
                                {
                                    responseVm.Message = "Window " + AppConstant.NoDataFound;
                                    return responseVm;
                                }

                                TblEstimateWindow tblEstimateWindow = new TblEstimateWindow
                                {
                                    EstWindowId = Guid.NewGuid(),
                                    EstimateId = CreateEstimate.EstimateId,
                                    WindowId = windowData.WindowId,
                                    IsActive = true,
                                    IsDelete = false,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = AppConstant.LoggedInUserID,
                                    TenantId = AppConstant.LoggedInTenantId,
                                    Label = windowData.Label,
                                    AddressId = windowData.AddressId,
                                    ArchHeight = windowData.ArchHeight,
                                    Height = windowData.Height,
                                    Width = windowData.Width,
                                    Quantity = windowData.Quantity,
                                    Notes = windowData.Notes,
                                };

                                await estimateWindowService.Add(tblEstimateWindow);

                                var productData = productService.GetAllQuerable().Where(x => x.ProductId == WindowProductData.ProductId && x.IsDelete == false).FirstOrDefault();
                                if(productData == null)
                                {
                                    responseVm.Message = "Product " + AppConstant.NoDataFound;
                                    return responseVm;
                                }

                                TblEstimateWindowProduct tblEstimateWindowProduct = new TblEstimateWindowProduct
                                {
                                   WindowId = (Guid)WindowProductData.WindowId,
                                   ProductName = productData.ProductName,
                                   Height = productData.Height,
                                   Width = productData.Width,
                                   AdditionalWidth = productData.AdditionalWidth,
                                   Notes = productData.Notes,
                                   IsDelete = false,
                                   IsActive = true,
                                   CreatedAt = DateTime.Now,
                                   CreatedBy = AppConstant.LoggedInUserID,
                                   ArchHeight = productData.ArchHeight,
                                   Area = productData.Area,
                                   Color = productData.Color,
                                   EstimateId = CreateEstimate.EstimateId,
                                   Length = productData.Length,
                                   Markup = productData.Markup,
                                   ProductType = productData.ProductType,
                                   TenantId = AppConstant.LoggedInTenantId,
                                   ProductId = productData.ProductId,
                                   EstimateWindowProductId = Guid.NewGuid()
                                };
                                await estimateWindowProductService.Add(tblEstimateWindowProduct);

                                //var componentData = componentService.GetAllQuerable().Where(x=>x.)

                            }
                        }
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.InsertSuccessMsg;
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Update_Estimate(UpdatetEstimate model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (IsTenantExists)
                {
                    var Estimate = GetAllQuerable()
                        .Where(c => c.EstimateId == model.EstimateId && c.TenantId == AppConstant.LoggedInTenantId
                        && c.IsDelete == false).FirstOrDefault();

                    var IsEstimateExists = GetAllQuerable()
                        .Where(c => c.EstimateId != model.EstimateId && c.EstimateName == model.EstimateName
                        && c.TenantId == AppConstant.LoggedInTenantId
                        && c.IsDelete == false && c.JobId != model.JobId).ToList();
                    if (IsEstimateExists.Count > 0)
                    {
                        responseVm.Message = "Estimate Name " + AppConstant.AlreadyExists;
                        return responseVm;
                    }


                    if (Estimate != null)
                    {
                        Estimate.EstimateName = model.EstimateName;
                        Estimate.Notes = model.Notes;
                        Estimate.DiscountAmount = model.Discount;
                        Estimate.GrandTotal = model.GrandTotal;
                        Estimate.JobId = model.JobId;
                        Estimate.TaxAmount = model.Tax;
                        Estimate.TermsCondition = model.TermsCondition;
                        Estimate.ModifiedBy = AppConstant.LoggedInUserID;
                        Estimate.UpdatedAt = DateTime.Now;

                        await Update(Estimate);

                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
        public async Task<IResponseVm<string>> ActiveInActiveEstimate(Guid EstimateId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var Estimate = GetAllQuerable()
                    .Where(c => c.EstimateId == EstimateId
                    && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false).FirstOrDefault();
                if (Estimate == null)
                {
                    responseVm.Message = "Estimates " + AppConstant.NoDataFound;
                    return responseVm;
                }
                Estimate.IsActive = !Estimate.IsActive;
                Estimate.UpdatedAt = DateTime.Now;
                Estimate.ModifiedBy = AppConstant.LoggedInUserID;
                await Update(Estimate);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
        public async Task<IResponseVm<string>> DeleteEstimate(Guid EstimateId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var Estimate = GetAllQuerable()
                    .Where(c => c.EstimateId == EstimateId
                    && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false).FirstOrDefault();
                if (Estimate == null)
                {
                    responseVm.Message = "Estimates " + AppConstant.NoDataFound;
                    return responseVm;
                }
                Estimate.IsActive = false;
                Estimate.IsDelete = true;
                Estimate.UpdatedAt = DateTime.Now;
                Estimate.ModifiedBy = AppConstant.LoggedInUserID;
                await Update(Estimate);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<List<EstimateList>>> GetAll_Estimate(Pagination filter)
        {
            IResponseVm<List<EstimateList>> responseVm = new ResponseVm<List<EstimateList>>();
            try
            {
                var Estimates = new List<TblEstimate>();
                List<EstimateList> EstimateList = new List<EstimateList>();
                if (filter.TenantId != "")
                {
                    var IsRealtedTenant = tenantService.GetAllQuerable().Where(x => x.TenantId.ToString() == filter.TenantId && x.ParentTenantId == AppConstant.LoggedInTenantId && x.IsDelete == false).FirstOrDefault();
                    if (IsRealtedTenant == null)
                    {
                        responseVm.Message = AppConstant.TenantNotFound;
                        return responseVm;
                    }
                    else
                    {
                        Estimates = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId.ToString() == filter.TenantId).OrderByDescending(x => x.CreatedAt).ToList();
                    }
                }
                else
                {
                    Estimates = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).OrderByDescending(x => x.CreatedAt).ToList();
                }


                foreach (var Estimate in Estimates)
                {
                    EstimateList e = new EstimateList
                    {
                        EstimateId = Estimate.EstimateId,
                        CreatedDate = Estimate.CreatedAt,
                        IsActive = Estimate.IsActive,
                        Discount = Estimate.DiscountAmount,
                        EstimateName = Estimate.EstimateName,
                        EstimateCode = Estimate.EstimateCode,
                        GrandTotal = Estimate.GrandTotal,
                        Notes = Estimate.Notes,
                        Tax = Estimate.TaxAmount,
                        TermsCondition = Estimate.TermsCondition,
                    };

                    EstimateList.Add(e);
                }
                filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                var FilterRecords = EstimateList
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (filter.SearchValue != "" ?
                                    ((x.Notes ?? "").Contains(filter.SearchValue)
                                    || (x.JobId.ToString() ?? "").Contains(filter.SearchValue)
                                    || (x.EstimateId.ToString()).Contains(filter.SearchValue)
                                    || (x.EstimateCode ?? "").Contains(filter.SearchValue)
                                    || (x.EstimateName ?? "").Contains(filter.SearchValue)
                                    || (x.Notes ?? "").Contains(filter.SearchValue)
                                    || (x.TermsCondition ?? "").Contains(filter.SearchValue)
                                    || (x.DiscountCoupon ?? "").Contains(filter.SearchValue)
                                    || (x.GrandTotal.ToString() ?? "").Contains(filter.SearchValue)
                                    ) : true)).ToList();
                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();
                responseVm.IsSuccess = true;

                if (TakenRecords != null && TakenRecords.Count > 0)
                {
                    responseVm.Data = TakenRecords.ToList();
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                    responseVm.TotalRecords = FilterRecords.Count;
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

    }
}
