using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.GHL.Services;
using ShutterManagement.Services;
using ShutterManagment.GHL.ViewModel.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class AppointmentScheduleService : BaseRepository<ShutterDBContext, TblAppointmentSchedule>, IAppointmentScheduleService
    {
        private readonly ITenantService tenantService;
        private readonly ClaimsProperty claimsProperty;

        public AppointmentScheduleService(ITenantService tenantService,ClaimsProperty claimsProperty)
        {
            this.tenantService = tenantService;
            this.claimsProperty = claimsProperty;
        }

        public async Task<IResponseVm<string>> CreateAppointmentSchedule(CreateAppointment model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var IsExist = GetAllQuerable().Where(x => x.JobId == model.JobId && x.IsActive == true && x.IsDelete == false).FirstOrDefault();
                if (IsExist != null)
                {
                    IsExist.JobId = model.JobId;
                    IsExist.ScheduleDateTime = model.ScheduleDateTime;
                    IsExist.Title= model?.Title?.Trim()??"";
                    IsExist.Description= model?.Description?.Trim() ?? "";
                    IsExist.UpdatedAt = DateTime.Now;
                    IsExist.ModifiedBy = AppConstant.LoggedInUserID;

                    await Update(IsExist);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                   
                }
                else
                {

                    TblAppointmentSchedule tblAppointmentSchedule = new TblAppointmentSchedule
                    {
                        Id = Guid.NewGuid(),
                        JobId = model.JobId,
                        ScheduleDateTime = model?.ScheduleDateTime,
                        Title = model?.Title?.Trim() ?? "",
                        Description = model?.Description?.Trim() ?? "",
                        CreatedAt = DateTime.Now,
                        TenantId = AppConstant.LoggedInTenantId,
                        CreatedBy = AppConstant.LoggedInUserID,
                        IsActive = true,
                        IsDelete = false
                    };
                    var res = await Add(tblAppointmentSchedule);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<AppointmentScheduleVm>>> GetAppointmentSchedule(Guid? JobId)
        {
            IResponseVm<List<AppointmentScheduleVm>> responseVm = new ResponseVm<List<AppointmentScheduleVm>>();

            try
            {
                var mappings = GetAllQuerable()
                    .Where(x => x.JobId==JobId &&  x.TenantId == AppConstant.LoggedInTenantId)
                    .Select(x => new AppointmentScheduleVm
                    {
                        JobId = x.JobId ?? Guid.Empty,
                        ScheduleDateTime = x.ScheduleDateTime,
                        Title = x.Title??"",
                        Description = x.Description??"",
                    })
                    .ToList();

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.FetchSuccessMsg;
                responseVm.Data = mappings;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return responseVm;
        }
    }

}
