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
    public class JobStatusService : BaseRepository<BtprojecShuttermanagementDevContext, MstJobStatus>, IJobStatusService
    {
        public async Task<IResponseVm<List<JobStatusList>>> GetAll(Pagination filter)
        {
            IResponseVm<List<JobStatusList>> responseVm = new ResponseVm<List<JobStatusList>>();
            try
            {
                var JobStatusList = (await GetAll()).Where(x => x.IsActive == true && x.IsDelete == false).ToList();
                                        

                    filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                    var FilterRecords = JobStatusList
                              .Where(x =>
                              (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                              (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                 (x.JobStatusId.ToString() == filter.SearchValue) ||
                                 (x.JobStatusName ?? "").ToLower().Contains(filter.SearchValue)
                                )
                             ).Select(x => new JobStatusList
                             {
                                 JobStatusId = x.JobStatusId,
                                 JobStatusName = x.JobStatusName,
                                 IsActive = x.IsActive
                             }).ToList();



                var TakenRecords = FilterRecords
                        .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                        .Take(filter.RecordPerPage)
                        .ToList();
                    responseVm.IsSuccess = true;

                    if (TakenRecords != null && TakenRecords.Count > 0)
                    {
                        responseVm.Data = TakenRecords.ToList();
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                        responseVm.TotalRecords = FilterRecords.Count;
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                    }
               
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }
    }
}
