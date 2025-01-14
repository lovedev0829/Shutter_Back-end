using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;

namespace ShutterManagement.Business.Services
{
    public class ReportServices : IReportServices
    {
        private readonly IOrderService orderService;
        private readonly IContactService contactService;

        public ReportServices(IOrderService orderService,IContactService contactService)
        {
            this.orderService = orderService;
            this.contactService = contactService;
        }

        Pagination filter = new Pagination();
        public async Task<IResponseVm<List<OrderReportList>>> GetOrderConfirmed()
        {
            IResponseVm<List<OrderReportList>> responseVm = new ResponseVm<List<OrderReportList>>();

            try
            {
                // Assuming 'filter' is already defined or passed into the method.
                var orders = await orderService.FetchOrders(filter);

                if (orders != null && orders.Any())
                {
                    // Filter orders by the status of "PendingForApproval".
                    var confirmedOrders = orders
                        .Where(o => o.Status == EnumValue.GetEnumValue(Estimation.Approved)) 
                        .OrderByDescending(o => o.CreatedAt)
                        .GroupBy(o => o.EstimateId)
                        .Select(g => g.FirstOrDefault()) // Get the first order in each group (distinct EstimateId)
                        .ToList();

                    var Contactname = 
                    // Mapping orders to OrderReportList DTO (you need to implement the mapping logic)
                    responseVm.Data = confirmedOrders.Select(order => new OrderReportList
                    {
                        OrderNo = order.OrderName,
                        EstimateId = order.EstimateId,
                        Confirmdate = order.UpdatedAt,
                        Orderdate = order.CreatedAt,
                        // Add any other required fields for `OrderReportList`
                    }).ToList();
                }
                else
                {
                    responseVm.Message = "No orders found.";
                 //   responseVm.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = $"An error occurred: {ex.Message}";
                //responseVm.Status = false;
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
