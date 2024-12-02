using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Services;
using ShutterManagement.DAL.Context;
using ShutterManagement.Business.Interfaces;

namespace ShutterManagement.Business.Services
{
    public class JobTypeService : BaseRepository<ShutterDbContext, MstJobType>, IJobTypeService
    {
        public async Task<IResponseVm<List<JobTypeList>>> GetAll(Pagination filter)
        {
            IResponseVm<List<JobTypeList>> responseVm = new ResponseVm<List<JobTypeList>>();
            try
            {
                var JobTypeList = (await GetAll()).Where(x => x.IsActive == true && x.IsDelete == false).ToList();


                filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                var FilterRecords = JobTypeList
                          .Where(x =>
                          (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                          (string.IsNullOrEmpty(filter.SearchValue) ? true :
                             (x.Id.ToString() == filter.SearchValue) ||
                             (x.JobTypeName ?? "").ToLower().Contains(filter.SearchValue)
                            )
                         ).Select(x => new JobTypeList
                         {
                             JobTypeId = x.Id,
                             JobTypeName = x.JobTypeName,
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
