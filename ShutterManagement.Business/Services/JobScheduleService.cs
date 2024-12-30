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

    public class JobScheduleService : BaseRepository<BtprojecShuttermanagementDevContext, TblJobSchedule>, IJobSchedule
    {
        private readonly ITenantService tenantService;
        private readonly ClaimsProperty claimsProperty;

        public JobScheduleService(ITenantService tenantService,ClaimsProperty claimsProperty)
        {
            this.tenantService = tenantService;
            this.claimsProperty = claimsProperty;
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
                

                TblJobSchedule tblJobSchedule = new TblJobSchedule
                {
                    JobScheduleId = Guid.NewGuid(),
                    JobTitleId = model.JobTitleId,
                    UserId = model.UserId,
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
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.InsertSuccessMsg;

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
                                       where i.IsDelete == false
                                       orderby i.CreatedAt descending
                                       select new JobScheduleVm
                                       {
                                           ScheduleJobId = i.JobScheduleId,
                                           JobTitleId = i.JobTitleId,
                                           JobTitle= jt.JobTitleName, 
                                           UserId = i.UserId,
                                           User = u.Username,
                                           StartDate = i.StartDate,
                                           EndDate = i.EndDate,
                                           DayStartTime = i.DayStartTime,
                                           DayEndDateTime = i.DayEndDateTime,
                                           TenantId= i.TenantId,
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
                                x.User.ToLower().Contains(filter.SearchValue) ||       
                                x.TenantId.ToString().Contains(filter.SearchValue)
                            )
                        )
                    ).ToList();


                var paginatedInvoices = filteredJobSchedul
                    .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                    .Take(filter.RecordPerPage)
                    .ToList();

                responseVm.IsSuccess = true;
                responseVm.Data = paginatedInvoices;
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
                    .Where(c => c.JobScheduleId == model.JobScheduleId
                                && c.TenantId == AppConstant.LoggedInTenantId
                                && c.IsDelete == false)
                    .FirstOrDefault();

                if (JobSchedule == null)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

                JobSchedule.JobTitleId = model.JobTitleId;
                JobSchedule.UserId = model.UserId;
                JobSchedule.StartDate = model.StartDate;
                JobSchedule.EndDate = model.EndDate;
                JobSchedule.DayStartTime = TimeOnly.Parse(model.DayStartTime);
                JobSchedule.DayEndDateTime = TimeOnly.Parse(model.DayEndDateTime);
                JobSchedule.UpdatedAt = DateTime.Now;
                JobSchedule.ModifiedBy = AppConstant.LoggedInUserID;

                await Update(JobSchedule);

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

    }
}
    
