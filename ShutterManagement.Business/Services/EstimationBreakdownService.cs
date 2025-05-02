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
                var res = await contactService.GetAll_Contact(filter, false);
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
                var res = await contactService.GetAll_Contact(filter, false);
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
            IResponseVm<List<WindowProdcutDetailsForEstimate>> responseVm = new ResponseVm<List<WindowProdcutDetailsForEstimate>>();

            try
            {
                // Step 1: Get relevant window product IDs for the estimate
                Guid[] windowProductIds = estimateWindowProductService.GetAllQuerable()
                    .Where(x => x.EstimateId == EstimationId)
                    .Select(x => x.WindowProductId)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToArray();

                // Step 2: Build estimate from service
                var estimateResponse = await estimateService.BuildEstimate(windowProductIds, true, EstimationId.ToString(), true);

                // Step 3: Filter if WindowProductId(s) provided
                List<WindowProdcutDetailsForEstimate> filteredList = new List<WindowProdcutDetailsForEstimate>();

                if (!string.IsNullOrWhiteSpace(WindowProductId))
                {
                    List<Guid> guidList = WindowProductId.Split(',')
                        .Select(id => Guid.TryParse(id.Trim(), out var guid) ? guid : Guid.Empty)
                        .Where(guid => guid != Guid.Empty)
                        .ToList();

                    foreach (var guid in guidList)
                    {
                        var matchedItem = estimateResponse.Data
                            .FirstOrDefault(x => x.WindowProdcutId.HasValue && x.WindowProdcutId.Value == guid);

                        if (matchedItem != null)
                        {
                            filteredList.Add(new WindowProdcutDetailsForEstimate
                            {
                                IsView = matchedItem.IsView,
                                WindowId = matchedItem.WindowId,
                                AddressId = matchedItem.AddressId,
                                Label = matchedItem.Label,
                                Width = matchedItem.Width,
                                Height = matchedItem.Height,
                                ArchHeight = matchedItem.ArchHeight,
                                Quantity = matchedItem.Quantity,
                                JobId = matchedItem.JobId,
                                IsRated = matchedItem.IsRated,
                                WindowProdcutId = matchedItem.WindowProdcutId,
                                ProductId = matchedItem.ProductId,
                                BuildWidth = matchedItem.BuildWidth,
                                BuildHeight = matchedItem.BuildHeight,
                                ProductName = matchedItem.ProductName,
                                ProductWidth = matchedItem.ProductWidth,
                                ProductHeight = matchedItem.ProductHeight,
                                ProductArchHeight = matchedItem.ProductArchHeight,
                                Area = matchedItem.Area,
                                Length = matchedItem.Length,
                                AdditionalWidth = matchedItem.AdditionalWidth,
                                Markup = matchedItem.Markup,
                                Options = matchedItem.Options,
                                OptionsPrice = matchedItem.OptionsPrice,
                                AdditionalCost = matchedItem.AdditionalCost,
                                ProductTypeName = matchedItem.ProductTypeName,
                                ComponentId = matchedItem.ComponentId,
                                ComponentQuantity = matchedItem.ComponentQuantity,
                                ComponentName = matchedItem.ComponentName,
                                MarkupPercentage = matchedItem.MarkupPercentage,
                                Price = matchedItem.Price,
                                PricingExpression = matchedItem.PricingExpression,
                                ExpressionFormulae = matchedItem.ExpressionFormulae,
                                RawMaterialId = matchedItem.RawMaterialId,
                                MaterialQuantity = matchedItem.MaterialQuantity,
                                WasteFactor = matchedItem.WasteFactor,
                                MaterialName = matchedItem.MaterialName,
                                UnitCost = matchedItem.UnitCost,
                                Non_Rated_UnitCost = matchedItem.Non_Rated_UnitCost,
                                UnitOfMeasurement = matchedItem.UnitOfMeasurement,
                                StockQuantity = matchedItem.StockQuantity,
                                ReorderLevel = matchedItem.ReorderLevel,
                                IsCutSheet = matchedItem.IsCutSheet,
                                TotalPrice = matchedItem.TotalPrice,
                                IsTaxApplied = matchedItem.IsTaxApplied
                            });
                        }
                    }
                }
                else
                {
                    filteredList = estimateResponse.Data;
                }

                // Step 4: Return final response
                responseVm.IsSuccess = true;
                responseVm.Data = filteredList;
                responseVm.Message = AppConstant.FetchSuccessMsg;
                responseVm.TotalRecords = filteredList.Count;
            }
            catch (Exception ex)
            {
                responseVm.Message = ex.Message;
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
