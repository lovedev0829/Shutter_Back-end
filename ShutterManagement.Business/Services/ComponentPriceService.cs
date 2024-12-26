using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels;
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
        public async Task GetAllComponentsChip(string Product)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var res = (await GetAll()).Where(x => x.ProductType == Product && x.IsActive == true).ToList();

            }
            catch (Exception ex) { 

            }
        }
    }
}
