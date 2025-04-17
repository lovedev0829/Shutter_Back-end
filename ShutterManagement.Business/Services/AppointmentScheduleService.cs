using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.GHL.Services;
using ShutterManagement.Services;
using ShutterManagment.GHL.ViewModel.Request;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class AppointmentScheduleService : BaseRepository<ShutterDBContext, TblAppointmentSchedule>, IAppointmentScheduleService
    {
        private readonly ITenantService tenantService;
        private readonly ClaimsProperty claimsProperty;
        private readonly JobStatusAndStageMapping statusAndStageMapping;
        private readonly IGHLConfiguration gHLConfiguration;
        private readonly GHLService_v2 gHLService_V2;

        public AppointmentScheduleService(ITenantService tenantService,ClaimsProperty claimsProperty,
            JobStatusAndStageMapping statusAndStageMapping,IGHLConfiguration gHLConfiguration,GHLService_v2 gHLService_V2)
        {
            this.tenantService = tenantService;
            this.claimsProperty = claimsProperty;
            this.statusAndStageMapping = statusAndStageMapping;
            this.gHLConfiguration = gHLConfiguration;
            this.gHLService_V2 = gHLService_V2;
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
                var ghlUserId = dbContext.TblUsers.Where(x => x.Id == model.UserId).Select(x => x.GhlUserId).FirstOrDefault();

                var IsExist = GetAllQuerable().Where(x => x.JobId == model.JobId && x.IsActive == true && x.IsDelete == false).FirstOrDefault();
                if (IsExist != null)
                {
                    IsExist.JobId = model.JobId;
                    IsExist.ScheduleDateTime = model.ScheduleDateTime;
                    IsExist.Title= model?.Title?.Trim()??"";
                    IsExist.Description= model?.Description?.Trim() ?? "";
                    IsExist.GhlUserId= ghlUserId ?? "";
                    IsExist.UserId= model?.UserId ?? null;
                    IsExist.UpdatedAt = DateTime.Now;
                    IsExist.ModifiedBy = AppConstant.LoggedInUserID;

                    await Update(IsExist);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                    if (responseVm.IsSuccess)
                    {
                        await gHLConfiguration.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                        var Checkghlresponse = await gHLConfiguration.CheckGHLConfiguration();
                        if (Checkghlresponse.IsSuccess)
                        {
          
                            string formattedSchedule = FormatDateTimeWithOffset(model?.ScheduleDateTime);
                            var appointment = new GHLAppointmentAdd
                            {
                                assignedUserId= IsExist.GhlUserId ?? "",
                                startTime = formattedSchedule ?? "",
                                locationId = AppConstant.GHL_Locationd ?? "",
                                ignoreDateRange = true,
                            };
                            var ghlResponse = await gHLService_V2.UpdateAppointmentAsync(IsExist?.GhlEventId, appointment);
                        }
                        else
                        {
                            var jobStatusName = EnumValue.GetEnumValue(JobStatus.Appointment_Scheduled);
                            var updateResponse = await statusAndStageMapping.UpdateOpportunityAndJobStatus(IsExist?.JobId ?? Guid.Empty, jobStatusName);
                        }
                    }

                }
                else
                {
                    var ghlUserIdAdd = dbContext.TblUsers.Where(x => x.Id == model.UserId).Select(x => x.GhlUserId).FirstOrDefault();

                    TblAppointmentSchedule tblAppointmentSchedule = new TblAppointmentSchedule
                    {
                        Id = Guid.NewGuid(),
                        JobId = model.JobId,
                        ScheduleDateTime = model?.ScheduleDateTime,
                        Title = model?.Title?.Trim() ?? "",
                        Description = model?.Description?.Trim() ?? "",
                        GhlUserId = ghlUserIdAdd ?? "",
                        CreatedAt = DateTime.Now,
                        TenantId = AppConstant.LoggedInTenantId,
                        CreatedBy = AppConstant.LoggedInUserID,
                        IsActive = true,
                        IsDelete = false
                    };
                    var res = await Add(tblAppointmentSchedule);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                    if (responseVm.IsSuccess)
                    {
                        await gHLConfiguration.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                        var Checkghlresponse = await gHLConfiguration.CheckGHLConfiguration();
                       if(Checkghlresponse.IsSuccess)
                        {
                            var jobStatusName = EnumValue.GetEnumValue(JobStatus.Appointment_Scheduled);
                            var updateResponse = await statusAndStageMapping.UpdateOpportunityAndJobStatus(tblAppointmentSchedule?.JobId ?? Guid.Empty, jobStatusName);
                            var contactId = dbContext.TblJobs.Where(x => x.JobId.ToString().ToLower() == res.JobId.ToString().ToLower() && x.IsDelete == false).Select(x => x.GhlContactId).FirstOrDefault();
                            string formattedSchedule = FormatDateTimeWithOffset(model?.ScheduleDateTime);
                            var appointment = new GHLAppointmentAdd
                            {
                                Title="Shutter Measurement Appointment",
                                contactId = contactId,
                                startTime = formattedSchedule ?? "",
                                locationId = AppConstant.GHL_Locationd ?? "",
                                assignedUserId = tblAppointmentSchedule?.GhlUserId ?? "",
                                ignoreDateRange = true,
                            };
                            var ghlResponse = await gHLService_V2.CreateAppointmentAsync(appointment);
                            if (ghlResponse != null)
                            {
                                res.GhlEventId = ghlResponse?.Data?.id;
                                res.LastSync = DateTime.Now;
                                await Update(res);
                            }
                            if (!updateResponse.IsSuccess)
                            {
                                responseVm.IsSuccess = false;
                                responseVm.Message = updateResponse.Message;
                                return responseVm;
                            }
                       }
                        else
                        {
                            var jobStatusName = EnumValue.GetEnumValue(JobStatus.Appointment_Scheduled);
                            var updateResponse = await statusAndStageMapping.UpdateOpportunityAndJobStatus(tblAppointmentSchedule?.JobId ?? Guid.Empty, jobStatusName);
                        }
                    }
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
                        JobId = x.JobId ?? null,
                        UserId = x.UserId ?? null,
                        GhlUserId=x.GhlUserId??"",
                        GhlUserName = dbContext.TblGhlUsers.Where(y=>y.GhlUserId==x.GhlUserId && y.IsActive==true && y.IsDelete==false).Select(s=>s.GhlUserName).FirstOrDefault(), 
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

        private static string FormatDateTimeWithOffset(DateTime? dateTime)
        {
            if (dateTime == null)
                return string.Empty;

            // Example: IST (Indian Standard Time, UTC+5:30)
            var dateTimeOffset = new DateTimeOffset(dateTime.Value, TimeSpan.FromHours(5.5));
            return dateTimeOffset.ToString("yyyy-MM-ddTHH\\:mm\\:sszzz", CultureInfo.InvariantCulture);
        }

    }

}
