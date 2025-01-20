using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
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
    public class JobTitleService : BaseRepository<ShutterDBContext, MstJobTitle>, IJobTitleService
    {
        public async Task<IResponseVm<List<JobTitlesList>>> GetAll(Pagination filter)
        {
            IResponseVm<List<JobTitlesList>> responseVm = new ResponseVm<List<JobTitlesList>>();
            try
            {
                var JobTitleList = (await GetAll()).Where(x => x.IsDelete == false).OrderByDescending(x => x.CreatedAt).ToList();
                filter.SearchValue = (filter.SearchValue?.Trim() ?? "").ToLower();
                var FilterRecords = JobTitleList
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (filter.SearchValue != "" ?
                                    (x.Id.ToString() == filter.SearchValue) ||
                                    ((x.JobTitleName ?? "").ToLower().Contains(filter.SearchValue)
                                    ) : true)).ToList();
                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();
                if (TakenRecords != null && TakenRecords.Count > 0)
                {
                    responseVm.Data = TakenRecords.Select(x => new JobTitlesList
                    {
                        Id = x.Id,
                        Name = x.JobTitleName,
                        IsActive = x.IsActive,
                    }).ToList();
                    responseVm.IsSuccess = true;
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

