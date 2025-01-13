using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core;
using ShutterManagement.Business.Interfaces;

namespace ShutterManagement.Business.Services
{
    public class ReportServices
    {
        private readonly IOrderService orderService;

        public ReportServices(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        Pagination filter = new Pagination();

        public class OrderReportList
        {
            public Guid? EstimateId { get; set; }
            public string? OrderNo { get; set; }
            public string? ContactName { get; set; }
            public DateTime? Orderdate { get; set; }
            public DateTime? Confirmdate { get; set; }
            public decimal? Totalamnt { get; set; }
        }
        public async Task<IResponseVm<List<OrderReportList>>> GetOrder()
        {
            IResponseVm<List<OrderReportList>> responseVm = new ResponseVm<List<OrderReportList>>();
            try
            {
                var order = await orderService.GetLatestVersionOrders(filter);

                if (order.TotalRecords > 0)
                {
                    var orderReportList = new List<OrderReportList>();
                    foreach (var item in order.Data)
                    {
                        var o = new OrderReportList
                        {
                            EstimateId = item.EstimateId,
                            OrderNo = item.OrderNumber,
                            Orderdate = item.CreatedDate,
                            Confirmdate = item.UpdatedAt,
                            Totalamnt = item.GrandTotal
                        };

                        orderReportList.Add(o);
                    }

                    responseVm.Data = orderReportList;
                }
                else
                {
                    responseVm.Message = "No records found.";
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log("Exception in GetOrder method: ", ex);
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }


    }
}
