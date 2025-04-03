using ShutterManagement.Business.Interfaces;
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

namespace ShutterManagement.Business.Services
{

    public class StateService : BaseRepository<ShutterDBContext, MstState>, IStateService
    {
        private readonly ShutterDBContext dbContext;

        public StateService(ShutterDBContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<IResponseVm<List<GetAllStateVm>>> GetAll(Pagination filter)
        {

            IResponseVm<List<GetAllStateVm>> responseVm = new ResponseVm<List<GetAllStateVm>>();
            try
            {
                var tax = (await GetAll()).Where(x => x.IsDelete == false).ToList();

                responseVm.Data = tax.Select(x => new GetAllStateVm
                {
                    StateId = x.Id,
                    StateName = x.StateName,
                    StateCode = x.StateCode,
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
