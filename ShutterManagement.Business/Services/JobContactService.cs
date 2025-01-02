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
    public class JobContactService : BaseRepository<BtprojecShuttermanagementDevContext, TblJobContact>, IJobContactService
    {
        private readonly ITenantService tenantService;
        private readonly IMstContactType mstContactType;

        public JobContactService(ITenantService tenantService,IMstContactType mstContactType )
        {
            this.tenantService = tenantService;
            this.mstContactType = mstContactType;
        }

        public async Task<IResponseVm<string>> ActiveInActiveJobContact(Guid JobContactId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var isTenantValid = tenantService.GetAllQuerable()
                    .Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive==true)
                    .FirstOrDefault();

                if (isTenantValid == null)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var jobContact = GetAllQuerable().FirstOrDefault(x => x.JobContactId == JobContactId && x.IsActive==true);

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
                    .Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive==true)
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
                    ContactName = model.Contactname,
                    IsActive = true,
                    IsDelete=false,
                    CreatedAt = DateTime.Now,
                    TenantId = AppConstant.LoggedInTenantId,
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
                    .Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true)
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
                    x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (!isTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var jobContactsQuery = from x in dbContext.TblJobContacts.Where(x => x.IsDelete==false && x.TenantId == AppConstant.LoggedInTenantId)
                join c in dbContext.MstContactTypes
                on x.ContactTypeId equals c.MstContactId
                orderby x.CreatedAt
                select new JobContactVm
                {
                JobContactId = x.JobContactId,
                ContactTypeId = x.ContactTypeId,
                ContactTypeName = c.MstContactName,
                JobId = x.JobId,
                ContactName = x.ContactName,
                isActive = x.IsActive
                };

                filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                var filteredInvoices = jobContactsQuery
                    .Where(x =>
                        (filter.Status == 0 ? x.isActive == false : (filter.Status == 1 ? x.isActive == true : true)) &&
                        ((string.IsNullOrEmpty(filter.SearchValue) ? true :
                            (x.ContactName.Contains(filter.SearchValue.Trim())) ||
                            (x.JobId.ToString() == filter.SearchValue.Trim())) ||
                            (x.JobId.ToString() == filter.SearchValue.Trim())) ||
                            (x.ContactTypeId.ToString() == filter.SearchValue.Trim())).ToList();

                

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
                var jobContact = GetAllQuerable().FirstOrDefault(x => x.JobContactId == model.JobContactId);

                if (jobContact != null)
                {
                    jobContact.ContactTypeId = model.ContactTypeId;
                    jobContact.JobId = model.Jobid;
                    jobContact.ContactName = model.Contactname;
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
