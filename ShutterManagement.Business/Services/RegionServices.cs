using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using ShutterManagement.Core;

namespace ShutterManagement.Business.Services
{
    public class RegionServices : BaseRepository<ShutterDBContext, MstRegion>, IRegion
    {
        private readonly ShutterDBContext dbContext;

        public RegionServices(ShutterDBContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<IResponseVm<List<GetAllRegion>>> GetAll(Pagination filter)
        {

            IResponseVm<List<GetAllRegion>> responseVm = new ResponseVm<List<GetAllRegion>>();
            try
            {
                var tax = (await GetAll()).Where(x => x.IsDelete == false).ToList();

                responseVm.Data = tax.Select(x => new GetAllRegion
                {
                    RegionId = x.Id,
                    RegionName = x.RegionName,
                    IsActive = x.IsActive,
                }).ToList();
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.FetchSuccessMsg;
                responseVm.TotalRecords = tax.Count;

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

       
    }
}

