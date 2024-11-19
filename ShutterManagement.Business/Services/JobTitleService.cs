using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class JobTitleService : BaseRepository<ShutterDbContext, MstJobTitle>, IJobTitleService
    {
        public async Task<IResponseVm<dynamic>> GetAll(Pagination filter)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var JobTitleList = (await GetAll()).Where(x => x.IsDelete == false).ToList();
                filter.SearchValue = (filter.SearchValue.Trim() ?? "").ToLower();
                var FilterRecords = JobTitleList
                                    .Where(x =>
                                    (filter.SearchValue != "" ?
                                    ((x.JobTitleName ?? "").ToLower().Contains(filter.SearchValue)
                                    ) : true)).ToList();
                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();
                if (TakenRecords != null && TakenRecords.Count > 0)
                {
                    responseVm.Data = TakenRecords.Select(x => new
                    {
                        x.JobTitleName,
                        x.IsActive,
                    });
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

