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
    public class UnitofMeasurementService : BaseRepository<ShutterDbContext, MstUnitofMeasurement>, IUnitofMeasurementService
    {
        public  async Task<IResponseVm<List<UnitofMeasurementList>>> GetAll(Pagination filter)
        {
            IResponseVm<List<UnitofMeasurementList>> responseVm = new ResponseVm<List<UnitofMeasurementList>>();
            try
            {
                var JobTitleList = (await GetAll()).Where(x => x.IsDelete == false).ToList();
                filter.SearchValue = (filter.SearchValue?.Trim() ?? "").ToLower();
                var FilterRecords = JobTitleList
                                    .Where(x =>
                                     (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (filter.SearchValue != "" ?
                                    ((x.UnitofMeasurementName ?? "").ToLower().Contains(filter.SearchValue)
                                    ) : true)).ToList();
                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();
                if (TakenRecords != null && TakenRecords.Count > 0)
                {
                    responseVm.Data = TakenRecords.Select(x => new UnitofMeasurementList
                    {
                        UnitofMeasurementId = x.UnitofMeasurementId,
                        UnitofMeasurementName = x.UnitofMeasurementName,
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
