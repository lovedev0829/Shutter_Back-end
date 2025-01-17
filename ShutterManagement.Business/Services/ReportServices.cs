using System.Drawing;
using System.Linq;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;

namespace ShutterManagement.Business.Services
{
    public class ReportServices : IReportServices
    {
        private readonly BtprojecShuttermanagementDevContext dbContext;
        private readonly IJobTypeService jobTypeService;
        private readonly IEstimateService estimateService;
        private readonly IOrderService orderService;
        private readonly IContactService contactService;
        private readonly IJobService jobService;

        public ReportServices(BtprojecShuttermanagementDevContext dbContext,
            IJobTypeService jobTypeService,IEstimateService estimateService,
            IOrderService orderService, IContactService contactService, IJobService jobService)
        {
            this.dbContext = dbContext;
            this.jobTypeService = jobTypeService;
            this.estimateService = estimateService;
            this.orderService = orderService;
            this.contactService = contactService;
            this.jobService = jobService;
        }

        public async Task<IResponseVm<List<OrderList>>> GetOrderConfirmed(Pagination filter)
        {
            IResponseVm<List<OrderList>> responseVm = new ResponseVm<List<OrderList>>();

            try
            {
                var orders = await orderService.GetAll_Order(filter);

                if (orders.Data == null || !orders.Data.Any())
                {
                    responseVm.Message = "No orders found.";
                    return responseVm;
                }

                var filteredOrders = orders.Data
                    .Where(o => o.Status == EnumValue.GetEnumValue(Estimation.Approved))
                    
                    .GroupBy(x => x.EstimateId)
                    .Select(group => group
                    .OrderByDescending(o => o.CreatedDate)
                    .FirstOrDefault())
                   .ToList();

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.FetchSuccessMsg;
                responseVm.Data = filteredOrders;
                responseVm.TotalRecords = filteredOrders.Count;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"An error occurred: {ex.Message}";
            }
            return responseVm;
        }

        public async Task<IResponseVm<List<EstimateList>>> GetTaxReport(Pagination filter)
        {
            IResponseVm<List<EstimateList>> responseVm = new ResponseVm<List<EstimateList>>();

            try
            {
                var estimates = await estimateService.GetAll_Estimate(filter);

                if (estimates.Data == null || !estimates.Data.Any())
                {
                    responseVm.Message = "No orders found.";
                    return responseVm;
                }

                var filteredOrders = estimates.Data
                    .Where(o => o.Status == EnumValue.GetEnumValue(Estimation.Approved))
                    .GroupBy(x => x.JobId)
                    .Select(group => group
                    .OrderByDescending(o => o.CreatedDate)
                    .FirstOrDefault())
                   .ToList();

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.FetchSuccessMsg;
                responseVm.Data = filteredOrders;
                responseVm.TotalRecords = filteredOrders.Count;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"An error occurred: {ex.Message}";
            }
            return responseVm;

        }
     

        //public async Task<IResponseVm<List<OrderReportList>>> GetOrderConfirmed()
        //{
        //    IResponseVm<List<OrderReportList>> responseVm = new ResponseVm<List<OrderReportList>>();
        //    try
        //    {
        //        var order = await orderService.GetLatestVersionOrders(filter);

        //        if (order.TotalRecords > 0)
        //        {
        //            var orderReportList = new List<OrderReportList>();
        //            foreach (var item in order.Data)
        //            {
        //                var contactname = (await contactService.GetAll()).
        //                    Where(x => x.ContactId == Guid.Parse(item.ContactId) && x.IsDelete == false).Select(x => x.Name).FirstOrDefault();  
        //                var o = new OrderReportList
        //                {
        //                    EstimateId = item.EstimateId,
        //                    OrderNo = item.OrderNumber,
        //                    ContactName = contactname,
        //                    Orderdate = item.CreatedDate,
        //                    Confirmdate = item.UpdatedAt,
        //                    Totalamnt = item.GrandTotal
        //                };

        //                orderReportList.Add(o);
        //            }

        //            responseVm.Message = AppConstant.FetchSuccessMsg;
        //            responseVm.Data = orderReportList;
        //            responseVm.TotalRecords = orderReportList.Count;
        //        }
        //        else
        //        {
        //            responseVm.Message = "No records found.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        FileLogger.Log("Exception in GetOrder method: ", ex);
        //        responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
        //    }

        //    return responseVm;
        //}



    }
}
