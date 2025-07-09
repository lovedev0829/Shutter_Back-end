using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core;
using static ShutterManagement.Business.Services.ReportServices;
using System.ComponentModel;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;

namespace ShutterManagement.Business.Services
{
    public class EstimationBreakdownService : IEstimationBreakdownService
    {
        private readonly IContactService contactService;
        private readonly IJobService jobService;
        private readonly IEstimateService estimateService;
        private readonly IEstimateWindowProductService estimateWindowProductService;

        //  private readonly Pagination pagination;

        public EstimationBreakdownService(IContactService contactService, IJobService jobService, 
            IEstimateService estimateService,IEstimateWindowProductService estimateWindowProductService)
        {
            this.contactService = contactService;
            this.jobService = jobService;
            this.estimateService = estimateService;
            this.estimateWindowProductService = estimateWindowProductService;
        }

        public async Task<IResponseVm<List<ContactsList>>> GetDistinctActiveContacts(Pagination filter)
        {
            IResponseVm<List<ContactsList>> responseVm = new ResponseVm<List<ContactsList>>();
            try
            {
                var res = await contactService.GetAll_Contact(filter, false, false);
                if (res?.Data != null)
                {
                    var activeContacts = res.Data.Where(x => x.IsActive == true).ToList();
                    var distinctContacts = activeContacts
                        .GroupBy(x => (x.FirstName + x.LastName))
                        .Select(group => group.First())
                        .ToList();
                    responseVm.IsSuccess = true;
                    responseVm.Data = distinctContacts;
                    responseVm.TotalRecords = distinctContacts.Count;
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = ex.Message;    
            }
            return responseVm;
        }

        public async Task<IResponseVm<List<ContactBreakdown>>> GetContactUniqueId(Pagination filter)
        {
            IResponseVm<List<ContactBreakdown>> responseVm = new ResponseVm<List<ContactBreakdown>>();
            try
            {
                var res = await contactService.GetAll_Contact(filter, false,false);
                if (res?.Data != null)
                {
                    var activeContacts = res.Data.Where(x => x.IsActive == true).ToList();

                    var contactBreakdowns = activeContacts.Select(item => new ContactBreakdown
                    {
                        Name = item.FirstName + " " + item.LastName,
                        ContactId = item.ContactId,
                        ContactUniqueId = item.ContactUniqueId
                    }).ToList(); 

                    responseVm.IsSuccess = true;
                    responseVm.Data = contactBreakdowns;
                    responseVm.TotalRecords = contactBreakdowns.Count;
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<List<GetAllJobsSubtenant_tenant>>> GetContactJobsBreakdown(Pagination filter)
        {
            IResponseVm<List< GetAllJobsSubtenant_tenant>> responseVm = new ResponseVm<List<GetAllJobsSubtenant_tenant>>();
            try
            {
                var res = await jobService.GetAllJobsTenant_SubTenant(filter);
                if (res?.Data != null)
                {
                    var activeContacts = res.Data.Where(x => x.IsActive == true).ToList();
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.Data = activeContacts;
                    responseVm.TotalRecords += activeContacts.Count;
                }
            }
            catch(Exception ex) 
            {
                responseVm.Message = AppConstant.NoDataFound;
            }
            return responseVm;
        }

        //private IResponseVm<List<ContactBreakdown>> ContactUniqueIdBreakdown(Guid ContactId)
        //{
        //    IResponseVm<List<ContactBreakdown>> responseVm = new ResponseVm<List<ContactBreakdown>>();
        //    try
        //    {
        //        pagination.SearchValue = ContactId.ToString().ToLower();
        //        var res = GetDistinctActiveContacts(pagination);
        //        if (res != null && res.Result.Data != null)
        //        {
        //            var distinctContacts = res.Result.Data
        //                .Select(contact => new
        //                {
        //                    ContactUniqueId = contact.ContactUniqueId,
        //                    ContactId = contact.ContactId
        //                })
        //                .Cast<object>()
        //                .ToList();
        //            responseVm.IsSuccess = true;
        //            responseVm.Message = AppConstant.FetchSuccessMsg;
        //            responseVm.Data = distinctContacts;
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        responseVm.Message = AppConstant.NoDataFound;
        //    }

        //    return new List<object>();
        //}


        public async Task<IResponseVm<List<EstimateList>>> GetContactEstimationsBreakdown(Pagination filter)
        {
            IResponseVm<List<EstimateList>> responseVm = new ResponseVm<List<EstimateList>>();
            try
            {
                var res = await estimateService.GetAll_Estimate(filter);
                if (res?.Data != null)
                {
                    var activeEstimates = res.Data.Where(x => x.IsActive == true).ToList();
                    responseVm.IsSuccess = true;
                    responseVm.Data = activeEstimates;
                    responseVm.TotalRecords -= activeEstimates.Count;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message=AppConstant.NoDataFound;
            }
            return responseVm;
        }

        //public async Task<IResponseVm<List<WindowProdcutDetailsForEstimate>>> GetProductBreakdown(Guid EstimationId, string WindowProductId = "")
        //{
        //    IResponseVm<List<WindowProdcutDetailsForEstimate>> responseVm = new ResponseVm<List<WindowProdcutDetailsForEstimate>>();
        //    try
        //    {
        //       // var res = await estimateService.GetAll_Estimate(new Pagination { SearchValue = EstimationId.ToString()});
        //        Guid[] windowProductIds = estimateWindowProductService.GetAllQuerable()
        //            .Where(x => x.EstimateId == EstimationId)
        //            .Select(x => x.WindowProductId)
        //            .Where(id => id.HasValue)  
        //            .Select(id => id.Value)   
        //            .ToArray();

        //        var estimateResponse = await estimateService.BuildEstimate(windowProductIds, true, EstimationId.ToString(), true);
        //        List<Guid> guidList = WindowProductId.Split(',')
        //                   .Select(id => Guid.Parse(id.Trim()))
        //                   .ToList();
        //        if (guidList != null)
        //        {
        //            foreach (var item in guidList)
        //            {
        //                var estimatebreakdown = estimateResponse.Data.Where(item => item.WindowProdcutId.ToString() == item.WindowProdcutId.ToString());
        //            }
        //            }
        //        responseVm.IsSuccess = true;
        //        responseVm.Data = estimateResponse.Data;
        //        responseVm.Message = AppConstant.FetchSuccessMsg;
        //        responseVm.TotalRecords = estimateResponse.TotalRecords;
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.Message = ex.Message;
        //    }
        //    return responseVm;
        //}

        public async Task<IResponseVm<List<WindowProdcutDetailsForEstimate>>> GetProductBreakdown(Guid EstimationId, string WindowProductId = "")
        {
            var responseVm = new ResponseVm<List<WindowProdcutDetailsForEstimate>>();

            try
            {
                // Step 1: Get relevant window product IDs for the estimate
                Guid[] windowProductIds = estimateWindowProductService.GetAllQuerable()
                    .Where(x => x.EstimateId == EstimationId)
                    .Select(x => x.WindowProductId ?? Guid.Empty)
                    .Where(id => id != Guid.Empty)
                    .ToArray();

                // Step 2: Build estimate from service
                var estimateResponse = await estimateService.BuildEstimate(windowProductIds, false, false,EstimationId.ToString(), true);
                var estimateData = estimateResponse?.Data ?? new List<WindowProdcutDetailsForEstimate>();

                // Step 3: Filter if WindowProductId(s) provided
                var filteredList = new List<WindowProdcutDetailsForEstimate>();

                List<Guid> filterIds = new();

                if (!string.IsNullOrWhiteSpace(WindowProductId))
                {
                    filterIds = WindowProductId
                        .Split(',')
                        .Select(id => Guid.TryParse(id.Trim(), out var guid) ? guid : Guid.Empty)
                        .Where(guid => guid != Guid.Empty)
                        .ToList();
                }

                var itemsToBind = string.IsNullOrWhiteSpace(WindowProductId)
                    ? estimateData
                    : estimateData.Where(x => x.WindowProdcutId.HasValue && filterIds.Contains(x.WindowProdcutId.Value)).ToList();

                foreach (var item in itemsToBind.DistinctBy(x => x.ComponentId))
                {
                    filteredList.Add(new WindowProdcutDetailsForEstimate
                    {
                        IsView = item.IsView,
                        WindowId = item.WindowId,
                        AddressId = item.AddressId,
                        Label = item.Label,
                        Width = item.Width,
                        Height = item.Height,
                        ArchHeight = item.ArchHeight,
                        Quantity = item.Quantity,
                        JobId = item.JobId,
                        IsRated = item.IsRated,
                        WindowProdcutId = item.WindowProdcutId,
                        ProductId = item.ProductId,
                        BuildWidth = item.BuildWidth,
                        BuildHeight = item.BuildHeight,
                        ProductName = item.ProductName,
                        ProductWidth = item.ProductWidth,
                        ProductHeight = item.ProductHeight,
                        ProductArchHeight = item.ProductArchHeight,
                        Area = item.Area,
                        Length = item.Length,
                        AdditionalWidth = item.AdditionalWidth,
                        Markup = item.Markup,
                        Options = item.Options,
                        OptionsPrice = item.OptionsPrice,
                        AdditionalCost = item.AdditionalCost,
                        ProductTypeName = item.ProductTypeName,
                        ComponentId = item.ComponentId,
                        ComponentQuantity = item.ComponentQuantity,
                        ComponentName = item.ComponentName,
                        MarkupPercentage = item.MarkupPercentage,
                        Price = item.Price,
                        PricingExpression = item.PricingExpression,
                        ExpressionFormulae = item.ExpressionFormulae,
                        RawMaterialId = item.RawMaterialId,
                        MaterialQuantity = item.MaterialQuantity,
                        WasteFactor = item.WasteFactor,
                        MaterialName = item.MaterialName,
                        UnitCost = item.UnitCost,
                        Non_Rated_UnitCost = item.Non_Rated_UnitCost,
                        UnitOfMeasurement = item.UnitOfMeasurement,
                        ProductMarkup = item.ProductMarkup,
                        ComponentMarkup = item.ComponentMarkup,
                        OptionMarkup = item.OptionMarkup,
                        StockQuantity = item.StockQuantity,
                        ReorderLevel = item.ReorderLevel,
                        ProductPrice = item.ProductPrice,
                        IsCutSheet = item.IsCutSheet,
                        TotalPrice = item.ProductPrice,
                        IsTaxApplied = item.IsTaxApplied,
                        OptionTaxDetails = item.OptionTaxDetails,
                    });
                }

                // Step 4: Return final response
                responseVm.IsSuccess = true;
                responseVm.Data = filteredList;
                responseVm.Message = AppConstant.FetchSuccessMsg;
                responseVm.TotalRecords = filteredList.Count;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"Error: {ex.Message}";
                responseVm.IsSuccess = false;
                responseVm.Data = new List<WindowProdcutDetailsForEstimate>();
                responseVm.TotalRecords = 0;
            }

            return responseVm;
        }


        public async Task<IResponseVm<List<object>>> GetProductListForEstimation(Guid EstimationId)
        {
            var responseVm = new ResponseVm<List<object>>();

            try
            {
                var res = await GetProductBreakdown(EstimationId);

                if (res != null && res.IsSuccess && res.Data != null && res.Data.Any())
                {
                    var productList = res.Data
                        .GroupBy(item => item.ProductName) 
                        .Select(group => new
                        {
                            ProductId = group.First().ProductId,
                            ProductName = group.Key,  
                            WindowProductIds = group.Select(x => x.WindowProdcutId).Distinct().ToList()  
                        })
                        .Cast<object>()  
                        .ToList();

                    responseVm.IsSuccess = true;
                    responseVm.Data = productList;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.TotalRecords = productList.Count;
                }
                else
                {
                    responseVm.Message = "No products found for the given estimation.";
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = ex.Message;
            }

            return responseVm;
        }



    }

}
