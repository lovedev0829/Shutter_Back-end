using Newtonsoft.Json;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
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
    public class JobStageStatusAutomationHistoryRepository : BaseRepository<ShutterDBContext, TblJobStageStatusAutomAtionHistory>, IjobStageStatusAutomAtionHistory
    {
       
        public async Task<IResponseVm<string>> SaveHistory(TblJob job)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                if (job == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Job data is null.";
                    return responseVm;
                }

                var history = new TblJobStageStatusAutomAtionHistory
                {
                    HistoryId = Guid.NewGuid(),
                    JobJson = JsonConvert.SerializeObject(job)??"",
                    ChangedBy = AppConstant.LoggedInTenantId,
                    ChangedAt = DateTime.Now,
                    TenantId = AppConstant.LoggedInTenantId
                };

                var res = Add(history);
                responseVm.IsSuccess = true;
                responseVm.Message = "Job history saved successfully.";
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error saving job history: {ex.Message}";
                responseVm.Data = null;

            }

            return responseVm;
        }
    }

}
