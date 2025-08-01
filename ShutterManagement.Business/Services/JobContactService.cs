using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class JobContactService : BaseRepository<ShutterDBContext, TblJobContact>, IJobContactService
    {
        private readonly ITenantService tenantService;
        private readonly IMstContactType mstContactType;
        private readonly ClaimsProperty claimsProperty;

        public JobContactService(ITenantService tenantService,IMstContactType mstContactType ,ClaimsProperty claimsProperty)
        {
            this.tenantService = tenantService;
            this.mstContactType = mstContactType;
            this.claimsProperty = claimsProperty;
        }

        public async Task<IResponseVm<string>> ActiveInActiveJobContact(Guid JobContactId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = tenantService.GetAllQuerable()
                    .Where(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive==true)
                    .FirstOrDefault();

                if (isTenantValid == null)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var jobContact = GetAllQuerable().FirstOrDefault(x => x.JobContactId == JobContactId && x.IsDelete==false);

                if (jobContact != null)
                {
                    jobContact.IsActive = !jobContact.IsActive;
                    jobContact.ModifiedBy = AppConstant.LoggedInUserID;
                    jobContact.UpdatedAt=DateTime.Now;
                    await Update(jobContact);

                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                    responseVm.IsSuccess = true;
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    responseVm.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError;
                responseVm.IsSuccess = false;
            }

            return responseVm;
        }

        
        public async Task<IResponseVm<string>> Create_JobContact(CreateJobContact model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = tenantService.GetAllQuerable()
                    .Where(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive==true)
                    .FirstOrDefault();

                if (isTenantValid == null)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var newJobContact = new TblJobContact
                {
                    JobContactId = Guid.NewGuid(),
                    JobId=model.Jobid,
                    ContactTypeId = model.ContactTypeId,
                    ContactId = model.ContactId,
                    IsActive = true,
                    IsDelete=false,
                    CreatedAt = DateTime.Now,
                    TenantId = claimsProperty.GetTenantId(),
                    CreatedBy=AppConstant.LoggedInUserID
                };

                await Add(newJobContact);

                responseVm.Message = AppConstant.InsertSuccessMsg;
                responseVm.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError;
                responseVm.IsSuccess = false;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> DeleteJobContact(Guid? JobContactId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = tenantService.GetAllQuerable()
                    .Where(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true)
                    .FirstOrDefault();

                if (isTenantValid == null)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }
                var jobContact = GetAllQuerable().FirstOrDefault(x => x.JobContactId == JobContactId);

                if (jobContact != null)
                {
                    jobContact.IsDelete = true;
                    jobContact.ModifiedBy = AppConstant.LoggedInUserID;
                    jobContact.UpdatedAt = DateTime.Now;
                    await Update(jobContact);
                    responseVm.Message = AppConstant.DeleteSuccessMsg;
                    responseVm.IsSuccess = true;
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    responseVm.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message =    AppConstant.ExceptionError;
                responseVm.IsSuccess = false;
            }

            return responseVm;
        }

        public async Task<IResponseVm<List<JobContactVm>>> GetAll_JobContact(Pagination filter)
        {
            IResponseVm<List<JobContactVm>> responseVm = new ResponseVm<List<JobContactVm>>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                    x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var TenantId = claimsProperty.GetTenantId();

                var subTenantIds = new List<Guid>();
                if (string.IsNullOrEmpty(claimsProperty.GetParentTenantId()))
                {
                    subTenantIds = tenantService.GetAccessibleTenantIds(TenantId);
                }

                var jobContactsQuery = from x in dbContext.TblJobContacts
                                       join c in dbContext.MstContactTypes
                                       on x.ContactTypeId equals c.MstContactId
                                       join d in dbContext.TblContacts on x.ContactId equals d.ContactId
                                       join j in dbContext.TblJobs on x.JobId equals j.JobId
                                       where (x.TenantId == TenantId || subTenantIds.Contains(x.TenantId ?? Guid.Empty)) && x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId()
                                       orderby x.CreatedAt
                select new JobContactVm
                {
                JobContactId = x.JobContactId,
                ContactTypeId = x.ContactTypeId,
                ContactTypeName = c.MstContactName,
                JobId = x.JobId,
                ContactId = x.ContactId,
                ContactFirstname = d.FirstName,
                ContactLastname = d.LastName,
                IsActive = x.IsActive
                };


                jobContactsQuery= jobContactsQuery.Where(x=>filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true));

                filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                var filteredInvoices = jobContactsQuery
                    .Where(x =>
                        (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                        (string.IsNullOrEmpty(filter.SearchValue) ? true :
                            (x.ContactFirstname.ToLower() ?? "").Contains(filter.SearchValue.Trim().ToLower()) ||
                            ((x.ContactFirstname.ToLower() ?? "") + " " + (x.ContactLastname.ToLower() ?? "")).Contains(filter.SearchValue.Trim().ToLower()) ||
                            (x.ContactLastname.ToLower() ?? "").Contains(filter.SearchValue.Trim().ToLower()) ||
                            (x.JobId.ToString().ToLower() == filter.SearchValue) ||
                            (x.ContactTypeId.ToString() == filter.SearchValue.Trim()))).ToList();

                

                var totalRecords =  jobContactsQuery.Count();


                var jobContacts =  jobContactsQuery
                    .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                    .Take(filter.RecordPerPage)
                    .ToList();

                responseVm.IsSuccess = true;
                responseVm.Data = jobContacts;
                responseVm.TotalRecords = totalRecords;
                responseVm.Message = AppConstant.FetchSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> Update_JobContact(UpdateJobContact model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                   x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var jobContact = GetAllQuerable().FirstOrDefault(x => x.JobContactId == model.JobContactId);

                if (jobContact != null)
                {
                    jobContact.ContactTypeId = model.ContactTypeId;
                    jobContact.JobId = model.Jobid;
                    jobContact.ContactId = model.ContactId;
                    jobContact.UpdatedAt = DateTime.Now;
                    jobContact.ModifiedBy=AppConstant.LoggedInUserID;

                    await Update(jobContact);

                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                    responseVm.IsSuccess = true;
                }
                else
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    responseVm.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError;
                responseVm.IsSuccess = false;
            }

            return responseVm;
        }
    }
}
