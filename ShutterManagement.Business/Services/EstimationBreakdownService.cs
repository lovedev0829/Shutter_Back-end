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

namespace ShutterManagement.Business.Services
{
    public class EstimationBreakdownService : IEstimationBreakdownService
    {
        private readonly IContactService contactService;
        private readonly IJobService jobService;
        private readonly IEstimateService estimateService;
      //  private readonly Pagination pagination;
       
        public EstimationBreakdownService(IContactService contactService, IJobService jobService, IEstimateService estimateService)
        {
            this.contactService = contactService;
            this.jobService = jobService;
            this.estimateService = estimateService;
        }

        public async Task<IResponseVm<List<ContactsList>>> GetDistinctActiveContacts(Pagination filter)
        {
            IResponseVm<List<ContactsList>> responseVm = new ResponseVm<List<ContactsList>>();
            try
            {
                var res = await contactService.GetAll_Contact(filter);
                if (res?.Data != null)
                {
                    var activeContacts = res.Data.Where(x => x.IsActive == true).ToList();
                    var distinctContacts = activeContacts
                        .GroupBy(x => x.Name)
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

        public async Task<IResponseVm<List<WindowProdcutDetailsForEstimate>>> GetProductBreakdown(Guid EstimationId, string WindowProductId = "")
        {
            IResponseVm<List<WindowProdcutDetailsForEstimate>> responseVm = new ResponseVm<List<WindowProdcutDetailsForEstimate>>();
            try
            {
                var res = await estimateService.GetAll_Estimate(new Pagination { SearchValue = EstimationId.ToString() });
                var windowProductIds = res.Data.FirstOrDefault()?.WindowProductId;

                var estimateResponse = await estimateService.BuildEstimate(windowProductIds, true);

                if (!string.IsNullOrEmpty(WindowProductId))
                {
                    estimateResponse.Data = estimateResponse.Data.Where(item => item.WindowProdcutId.ToString() == WindowProductId).ToList();
                }

                responseVm.IsSuccess = true;
                responseVm.Data = estimateResponse.Data;
                responseVm.Message = AppConstant.FetchSuccessMsg;
                responseVm.TotalRecords = estimateResponse.TotalRecords;
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
                    var productList = res.Data.Select(item => new
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        WindowProductIds = item.WindowProdcutId
                    })
                    .Distinct() 
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
