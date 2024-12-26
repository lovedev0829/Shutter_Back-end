using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class ComponentPriceService : BaseRepository<BtprojecShuttermanagementDevContext, MstComponentPrice>, IComponentPriceService
    {
        public async Task<IResponseVm<dynamic>> GetAllComponentsChip(string Product)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var res = (await GetAll()).Where(x => x.ProductType == Product && x.IsActive == true).ToList();
                if (res.Count() > 0) {
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.Data = res.Select(x => new ComponentPrice
                    {
                        Id = x.Id,
                        PriceRated = (decimal)x.PriceRated,
                        PriceNonRated = (decimal)x.PriceNonRated,
                        ProductName = x.ProductType,
                        ComponentName = x.ComponentName,
                        Name = "MstComponentPrice." + x.ProductType + "." + x.ComponentName,
                    });
                }
            }
            catch (Exception ex) {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
    }
}
