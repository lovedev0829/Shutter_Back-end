using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core;
using ShutterManagement.DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Services;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Business.Interfaces;

namespace ShutterManagement.Business.Services
{
    public class MstContactTypeService : BaseRepository<BtprojecShuttermanagementDevContext, MstContactType>, IMstContactType
    {

        public async Task<IResponseVm<List<MstContactTypeVm>>> GetAll_MstContactType(Pagination filter)
        {
            IResponseVm<List<MstContactTypeVm>> responseVm = new ResponseVm<List<MstContactTypeVm>>();
            try
            {
                var mstContactTypeList = (await GetAll()).Where(x => x.IsDeleted == false).ToList();
                filter.SearchValue = (filter.SearchValue?.Trim() ?? "").ToLower();
                var FilterRecords = mstContactTypeList
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (filter.SearchValue != "" ?
                                    (x.MstContactId.ToString() == filter.SearchValue) ||
                                    ((x.MstContactName ?? "").ToLower().Contains(filter.SearchValue)
                                    ) : true)).ToList();
                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();
                if (TakenRecords != null && TakenRecords.Count > 0)
                {
                    responseVm.Data = TakenRecords.Select(x => new MstContactTypeVm
                    {
                        MstContactName = x.MstContactName,
                        IsActive = x.IsActive,
                    }).ToList();
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
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

 
