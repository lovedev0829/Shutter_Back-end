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
using ShutterManagement.GHL.Services;
using ShutterManagment.GHL.ViewModel.Request;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ShutterManagement.Core.Common;
using System.Data.Entity;

namespace ShutterManagement.Business.Services
{

    public class JobScheduleService : BaseRepository<ShutterDBContext, TblJobSchedule>, IJobSchedule
    {
        private readonly ITenantService tenantService;
        private readonly ClaimsProperty claimsProperty;
        private readonly IGHLConfiguration gHLConfiguration;
        private readonly GHLService_v2 gHLService_V2;
        private readonly JobStatusAndStageMapping statusAndStageMapping;

        public JobScheduleService(ITenantService tenantService,ClaimsProperty claimsProperty,
            IGHLConfiguration gHLConfiguration,GHLService_v2 gHLService_V2,JobStatusAndStageMapping statusAndStageMapping)
        {
            this.tenantService = tenantService;
            this.claimsProperty = claimsProperty;
            this.gHLConfiguration = gHLConfiguration;
            this.gHLService_V2 = gHLService_V2;
            this.statusAndStageMapping = statusAndStageMapping;
        }

        public async Task<IResponseVm<string>> ActiveInActive(Guid JobScheduleId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (isTenantValid)
                {
                    var jobschedule = GetAllQuerable()
                        .Where(c => c.JobScheduleId == JobScheduleId
                                   && c.TenantId == AppConstant.LoggedInTenantId
                                   && c.IsDelete == false)
                        .FirstOrDefault();

                    if (jobschedule != null)
                    {
                        jobschedule.IsActive = !jobschedule.IsActive;
                        jobschedule.UpdatedAt = DateTime.Now;
                        jobschedule.ModifiedBy = AppConstant.LoggedInUserID;

                        var result = await Update(jobschedule);
                    }
                    else
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.AccessDenied;
                }
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> CreateJobSchedule(CreateJobSchedule model)
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
                

                TblJobSchedule tblJobSchedule = new TblJobSchedule
                {
                    JobScheduleId = Guid.NewGuid(),
                    JobTitleId = model.JobTitleId,
                    UserId = model.UserId?? null,
                    JobId = model.JobId,
                    GhlUserId = ghlUserId ?? "",
                    EstimateId=model.EstimateId,
                    StartDate = model?.StartDate,
                    EndDate = model?.EndDate,
                    DayStartTime = TimeOnly.Parse(model.DayStartTime),
                    DayEndDateTime = TimeOnly.Parse(model.DayEndDateTime),
                    CreatedAt = DateTime.Now,
                    TenantId = AppConstant.LoggedInTenantId,
                    CreatedBy = AppConstant.LoggedInUserID,
                    IsActive = true,
                    IsDelete = false
                };
                var res = await Add(tblJobSchedule);

                var Checkghlresponse = await gHLConfiguration.CheckGHLConfiguration();
                await gHLConfiguration.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                if (Checkghlresponse.IsSuccess)
                {
                    var contactId = dbContext.TblJobs.Where(x => x.JobId.ToString().ToLower() == res.JobId.ToString().ToLower() && x.IsDelete == false).Select(x => x.GhlContactId).FirstOrDefault();

                    string formattedstartDate = FormatDateTimeWithOffset(res.StartDate,res.DayStartTime);
                    string formattedstartDateend = FormatDateTimeWithOffset(res.EndDate, res.DayEndDateTime); 

                    var appointment = new GHLAppointmentAdd
                    {
                        Title = "Shutter Installation Appointment",
                        contactId = contactId,
                        startTime = formattedstartDate,
                        endtime = formattedstartDateend,
                        locationId = AppConstant.GHL_Locationd,
                        assignedUserId = tblJobSchedule.GhlUserId??"",
                        ignoreDateRange = true,
                    };
                    var ghlResponse = await gHLService_V2.CreateAppointmentAsync(appointment);
                    if(ghlResponse != null)
                    {
                        res.GhlEventId = ghlResponse.Data.id;
                        res.LasSync = DateTime.Now;
                        await Update(res);
                    }
                    responseVm.IsSuccess = true;
                    responseVm.Message = Checkghlresponse.Message;
                }
                else
                {
                    responseVm.Message = Checkghlresponse.Message;
                }
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.InsertSuccessMsg;

                if (responseVm.IsSuccess)
                {
                    var jobStatusName = EnumValue.GetEnumValue(JobStatus.Installation_Scheduled);
                    var updateResponse = await statusAndStageMapping.UpdateOpportunityAndJobStatus(tblJobSchedule?.JobId ?? Guid.Empty, jobStatusName);

                    if (!updateResponse.IsSuccess)
                    {
                        FileLogger.Log(updateResponse.Message);
                        //responseVm.IsSuccess = false;
                        //responseVm.Message = updateResponse.Message;
                        //return responseVm;
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

        public async Task<IResponseVm<string>> DeleteJobSchedule(Guid JobScheduleId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (isTenantValid)
                {
                    var JobSchedule = GetAllQuerable()
                        .Where(c => c.JobScheduleId == JobScheduleId
                                   && c.TenantId == AppConstant.LoggedInTenantId
                                   && c.IsDelete == false)
                        .FirstOrDefault();

                    if (JobSchedule != null)
                    {
                        JobSchedule.IsDelete = !JobSchedule.IsDelete;
                        JobSchedule.UpdatedAt = DateTime.Now;
                        JobSchedule.ModifiedBy = AppConstant.LoggedInUserID;
                        var result = await Update(JobSchedule);
                    }
                    else
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.AccessDenied;
                }
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.DeleteSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<JobScheduleVm>>> GetAll_JobSchedule(Pagination filter)
        {
            IResponseVm<List<JobScheduleVm>> responseVm = new ResponseVm<List<JobScheduleVm>>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }

                var JobScheduleQuery = from i in dbContext.TblJobSchedules
                                       join jt in dbContext.MstJobTitles on i.JobTitleId equals jt.Id
                                       join u in dbContext.TblUsers on i.UserId equals u.Id
                                       join e in dbContext.TblEstimates on i.EstimateId equals e.EstimateId
                                       where i.IsDelete == false
                                       orderby i.CreatedAt descending
                                       select new JobScheduleVm
                                       {
                                           ScheduleJobId = i.JobScheduleId,
                                           JobTitleId = i.JobTitleId,
                                           JobTitle= jt.JobTitleName ?? "", 
                                           UserId = i.UserId,
                                           GhlUserId = i.GhlUserId ?? "",
                                           JobId = i.JobId,
                                           GhlUserName =dbContext.TblGhlUsers.Where(x => x.GhlUserId == i.GhlUserId).Select(x => x.GhlUserName).FirstOrDefault() ?? "",
                                           UserFullname = u.FirstName+ " " +u.LastName ?? "",
                                           StartDate = i.StartDate,
                                           EndDate = i.EndDate,
                                           DayStartTime = i.DayStartTime,
                                           DayEndDateTime = i.DayEndDateTime,
                                           TenantId= i.TenantId,
                                           EstimateId=i.EstimateId,
                                           EstimateName=e.EstimateName ?? "",
                                           IsActive = i.IsActive,
                                       };

                filter.SearchValue = filter.SearchValue?.Trim().ToLower();

                var filteredJobSchedul = JobScheduleQuery
                    .Where(x =>
                        (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                        (string.IsNullOrEmpty(filter.SearchValue) ? true :
                            (
                                x.JobTitleId.ToString().Contains(filter.SearchValue) || 
                                x.JobTitle.ToLower().Contains(filter.SearchValue) ||    
                                x.UserId.ToString().Contains(filter.SearchValue) ||    
                                x.UserFullname.ToLower().Contains(filter.SearchValue) ||       
                                x.TenantId.ToString().Contains(filter.SearchValue) ||
                                x.JobId.ToString().Contains(filter.SearchValue) 
                            )
                        )
                    ).ToList();


                var queryable = filteredJobSchedul.AsQueryable();

                queryable = Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                queryable = Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);

                var TakenRecords = Paginate.Pagination(queryable.ToList(), filter);

                responseVm.IsSuccess = true;
                responseVm.Data = TakenRecords;
                responseVm.TotalRecords = filteredJobSchedul.Count;
                responseVm.Message = AppConstant.FetchSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> UpdateJobSchedule(UpdateJobSchedule model)
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

                var JobSchedule = GetAllQuerable()
                    .Where(c => c.JobScheduleId == model.ScheduleJobId
                                && c.TenantId == AppConstant.LoggedInTenantId
                                && c.IsDelete == false)
                    .FirstOrDefault();

                if (JobSchedule == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }
                var ghlUserId = dbContext.TblUsers.Where(x => x.Id == model.UserId).Select(x => x.GhlUserId).FirstOrDefault();

                JobSchedule.JobTitleId = model.JobTitleId;
                JobSchedule.UserId = model.UserId;
                JobSchedule.GhlUserId = ghlUserId ?? "";
                JobSchedule.EstimateId = model.EstimateId;
                JobSchedule.StartDate = model.StartDate;
                JobSchedule.EndDate = model.EndDate;
                JobSchedule.DayStartTime = TimeOnly.Parse(model.DayStartTime);
                JobSchedule.DayEndDateTime = TimeOnly.Parse(model.DayEndDateTime);
                JobSchedule.UpdatedAt = DateTime.Now;
                JobSchedule.ModifiedBy = AppConstant.LoggedInUserID;

                await Update(JobSchedule);
                var Checkghlresponse = await gHLConfiguration.CheckGHLConfiguration();
                await gHLConfiguration.GetGHLConfiguration(AppConstant.LoggedInTenantId.ToString());
                if (Checkghlresponse.IsSuccess)
                {
                    if (JobSchedule.GhlEventId != null)
                    {

                        var contactId = dbContext.TblJobs.Where(x => x.JobId.ToString().ToLower() == JobSchedule.JobId.ToString().ToLower() && x.IsDelete == false).Select(x => x.GhlContactId).FirstOrDefault();

                        string formattedstartDate = FormatDateTimeWithOffset(JobSchedule.StartDate, JobSchedule.DayStartTime);
                        string formattedstartDateend = FormatDateTimeWithOffset(JobSchedule.EndDate, JobSchedule.DayEndDateTime);
                        var appointment = new GHLAppointmentAdd
                        {
                            Title = "Shutter Installation Appointment",
                            contactId = contactId,
                            startTime = formattedstartDate,
                            endtime = formattedstartDateend,
                            locationId = AppConstant.GHL_Locationd,
                            assignedUserId = model.GhlUserId ?? "",
                        };
                        var ghlResponse = await gHLService_V2.UpdateAppointmentAsync(JobSchedule.GhlEventId, appointment);
                    }
                    else
                    {
                        var contactId = dbContext.TblJobs.Where(x => x.JobId.ToString().ToLower() == JobSchedule.JobId.ToString().ToLower() && x.IsDelete == false).Select(x => x.GhlContactId).FirstOrDefault();

                        string formattedstartDate = FormatDateTimeWithOffset(JobSchedule.StartDate, JobSchedule.DayStartTime);
                        string formattedstartDateend = FormatDateTimeWithOffset(JobSchedule.EndDate, JobSchedule.DayEndDateTime);
                        var appointment = new GHLAppointmentAdd
                        {
                            Title = "Shutter Installation Appointment",
                            contactId = contactId,
                            startTime = formattedstartDate,
                            endtime = formattedstartDateend,
                            locationId = AppConstant.GHL_Locationd,
                            assignedUserId = model.GhlUserId,
                            ignoreDateRange = true,
                        };
                        var ghlResponse = await gHLService_V2.CreateAppointmentAsync(appointment);
                        if (ghlResponse != null)
                        {
                            JobSchedule.GhlEventId = ghlResponse.Data.id;
                            JobSchedule.LasSync = DateTime.Now;
                            await Update(JobSchedule);
                        }
                    }

                }

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
            
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        private static string FormatDateTimeWithOffset(DateTime? startDate,TimeOnly? startTime)
        {
           
            string inputStart = startDate.Value.ToString("yyyy-MM-dd") + " " + startTime.Value.ToString(@"hh\:mm");
            string format = "yyyy-MM-dd HH:mm";

            // Parse start time
            DateTime dateTimeStart = DateTime.ParseExact(inputStart, format, CultureInfo.InvariantCulture);
            DateTimeOffset dateTimeOffsetStart = new DateTimeOffset(dateTimeStart, TimeSpan.FromHours(5.5));
            string formattedStartDate = dateTimeOffsetStart.ToString("yyyy-MM-ddTHH\\:mm\\:sszzz", CultureInfo.InvariantCulture);
            return formattedStartDate ?? "";
        }

        public async Task<IResponseVm<string>> AppointmentSync(Guid? LoggedInTenantId = null)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                LoggedInTenantId = ((LoggedInTenantId != Guid.Empty && LoggedInTenantId != null) ? LoggedInTenantId : AppConstant.LoggedInTenantId);

                var Checkghlresponse = await gHLConfiguration.CheckGHLConfiguration(LoggedInTenantId);
                await gHLConfiguration.GetGHLConfiguration(LoggedInTenantId.ToString());
                if (Checkghlresponse.IsSuccess)
                {
                    var jobschedule = GetAllQuerable().Where(x => x.TenantId.ToString().ToLower() == LoggedInTenantId.ToString().ToLower()
                    && x.IsDelete == false).ToList();
                    if(jobschedule.Count==0)
                    {
                        responseVm.IsSuccess = true;
                        responseVm.Message = "Appointment not Found In Application";
                        return responseVm;
                    }
                    foreach(var item in jobschedule)
                    {
                        if(item.GhlEventId != null)
                        {
                          if(item.UpdatedAt > item.LasSync)
                          {
                          var contactId = await dbContext.TblJobs.Where(x => x.JobId.ToString().ToLower() == item.JobId.ToString().ToLower() && x.IsDelete == false).Select(x => x.GhlContactId).FirstOrDefaultAsync();
                          string formattedstartDate = FormatDateTimeWithOffset(item.StartDate, item.DayStartTime);
                          string formattedstartDateend = FormatDateTimeWithOffset(item.EndDate, item.DayEndDateTime);
                          var appointment = new GHLAppointmentAdd
                          {
                            contactId = contactId??"",
                            startTime = formattedstartDate??"",
                            endtime = formattedstartDateend ?? "",
                            locationId = AppConstant.GHL_Locationd??"",
                          };
                          var ghlResponse = await gHLService_V2.UpdateAppointmentAsync(item.GhlEventId, appointment);
                          if(!ghlResponse.IsSuccess)
                                {
                                FileLogger.Log("Error while creating Appointment.");
                                }
                          }
                            else
                            {
                                responseVm.IsSuccess = true;
                                responseVm.Message = "Appointment Already  Synced";
                                return responseVm;
                            }
                        }
                        else
                        {
                            var contactId = dbContext.TblJobs.Where(x => x.JobId.ToString().ToLower() == item.JobId.ToString().ToLower() && x.IsDelete == false).Select(x => x.GhlContactId).FirstOrDefault();
                            string formattedstartDate = FormatDateTimeWithOffset(item.StartDate, item.DayStartTime);
                            string formattedstartDateend = FormatDateTimeWithOffset(item.EndDate, item.DayEndDateTime);
                            var appointment = new GHLAppointmentAdd
                            {
                                contactId = contactId,
                                startTime = formattedstartDate,
                                endtime = formattedstartDateend,
                                locationId = AppConstant.GHL_Locationd,
                            };
                            var ghlResponse = await gHLService_V2.CreateAppointmentAsync(appointment);
                            if (!ghlResponse.IsSuccess)
                            {
                                FileLogger.Log("Error while creating Appointment.");
                            }
                        }
                    }
                }
                }
            catch(Exception ex)
            {
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }
    }
}
    
