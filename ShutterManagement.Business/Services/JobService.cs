using System.Linq;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using Microsoft.EntityFrameworkCore;
using static ShutterManagement.Core.ViewModels.Response.GetAllJobsSubtenant_tenant;

namespace ShutterManagement.Business.Services
{
    public class JobService : BaseRepository<BtprojecShuttermanagementDevContext, TblJob>, IJobService
    {
        private readonly BtprojecShuttermanagementDevContext _context;
        private readonly ClaimsProperty claimsProperty;
        private readonly IAddress address;
        private readonly ITenantService tenantService;

        public JobService(BtprojecShuttermanagementDevContext context, ClaimsProperty claimsProperty, IAddress address,
            ITenantService tenantService)
        {
            _context = context;
            this.claimsProperty = claimsProperty;
            this.address = address;
            this.tenantService = tenantService;
        }
        public async Task<IResponseVm<string>> ActiveInactive(string JobId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsTenantvalid = await tenantService.AnyAsync(x => x.IsActive == true && x.IsDelete == false
                && x.TenantId == AppConstant.LoggedInTenantId);
                if (!IsTenantvalid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsJobExist = GetAllQuerable().Where(x => x.JobId.ToString() == JobId &&
                  x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                if (IsJobExist != null)
                {
                    IsJobExist.IsActive = !IsJobExist.IsActive;
                    IsJobExist.UpdatedAt = DateTime.Now;
                    IsJobExist.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = await Update(IsJobExist);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
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

        public async Task<IResponseVm<string>> Create(CreateJob model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsTenantValid = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (!IsTenantValid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var generateUnique = GenerateUniqueId.GenerateUniqueIds(ModuleName.Job, AppConstant.LoggedInCompanyCode);
                bool IsAddressExist = await address.AnyAsync(x => x.AddressId == model.AddressId && x.ContactId == model.ContactId
                && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false);

                bool IsjobNameExists = await AnyAsync(x => x.JobName == model.JobName.Trim() && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId && x.ContactId == model.ContactId);
                if (IsjobNameExists)
                {
                    responseVm.Message = "JobName" + AppConstant.AlreadyExists;
                    return responseVm;
                }
                if (IsAddressExist)
                {
                    var jobStatus = _context.MstJobStatuses.Where(s => s.JobStatusName == "Pending" && s.IsActive == true).FirstOrDefault();
                    string arrayString = string.Join(", ", model.AttachmentsAny);
                    TblJob job = new TblJob
                    {
                        AddressId = model.AddressId,
                        ContactId = model.ContactId,
                        JobName = model.JobName?.Trim(),
                        JobType = model.JobType,
                        AssignedPersonnel = model.AssignedPersonnel,
                        ScheduledDate = model.ScheduledDate,
                        JobStatus = jobStatus?.JobStatusId,
                        Notes = model.Notes.Trim(),
                        AttachmentAny = arrayString,
                        JobUniqueId = generateUnique,
                        TenantId = AppConstant.LoggedInTenantId,
                        IsActive = true,
                        IsDelete = false,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID
                    };
                    var res = await Add(job);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                }
                else
                {
                    responseVm.Message = "Address " + AppConstant.NoDataFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> Delete(string JobId)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsTenantvalid = await tenantService.AnyAsync(x => x.IsActive == true && x.IsDelete == false
                && x.TenantId == AppConstant.LoggedInTenantId);
                if (!IsTenantvalid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsJobExist = GetAllQuerable().Where(x => x.JobId.ToString() == JobId &&
                   x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                if (IsJobExist != null)
                {
                    IsJobExist.IsActive = false;
                    IsJobExist.IsDelete = true;
                    IsJobExist.UpdatedAt = DateTime.Now;
                    IsJobExist.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = await Update(IsJobExist);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.DeleteSuccessMsg;
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

        public async Task<IResponseVm<List<GetAllJobsdetails>>> GetAll(Pagination filter)
        {

            IResponseVm<List<GetAllJobsdetails>> responseVm = new ResponseVm<List<GetAllJobsdetails>>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true);

                if (IsTenant)
                {

                    var JobList = (from t2 in _context.TblJobs.OrderByDescending(x => x.CreatedAt)
                                   join t1 in _context.TblAddresses on t2.AddressId equals t1.AddressId
                                   join t3 in _context.MstJobTypes on t2.JobType equals t3.Id
                                   join t4 in _context.TblUsers on t2.AssignedPersonnel equals t4.Id
                                   join t5 in _context.MstJobStatuses on t2.JobStatus equals t5.JobStatusId
                                   join t6 in _context.TblContacts on t2.ContactId equals t6.ContactId
                                   where t2.TenantId == AppConstant.LoggedInTenantId && t2.IsDelete == false && t6.IsActive == true && t6.IsDelete == false
                                   select new GetAllJobsdetails
                                   {
                                       IsActive = t2.IsActive,
                                       JobId = t2.JobId,
                                       JobName = t2.JobName,
                                       ContactId = t2.ContactId,
                                       ContactFirstName = t6.FirstName,
                                       ContactLasttName = t6.LastName,
                                       AddressId = t2.AddressId,
                                       JobType = t3.Id,
                                       JobTypeName = t3.JobTypeName,
                                       AssignedPersonnel = t4.Id,
                                       AssignedPersonnelName = t4.FirstName + " " + t4.LastName,
                                       ScheduledDate = t2.ScheduledDate,
                                       JobStatus = t5.JobStatusId,
                                       JobStatusName = t5.JobStatusName,
                                       Notes = t2.Notes,
                                       AttachmentsAny = t2.AttachmentAny.Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray(),
                                       Label = t1.Label,
                                       StreetAddress = t1.StreetAddress,
                                       City = t1.City,
                                       State = t1.State,
                                       ZipCode = t1.ZipCode,
                                       JobUniqueCode = t2.JobUniqueId,
                                   }).ToList();
                    filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                    var FilterRecords = JobList
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                      (string.IsNullOrEmpty(filter.JobStatus) ? true : (filter.JobStatus == x.JobStatus.ToString())) &&
                                      (string.IsNullOrEmpty(filter.Address) ? true : (filter.Address == x.AddressId.ToString())) &&
                                      (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                        (x.JobId.ToString() == filter.SearchValue) ||
                                        (x.ContactId.ToString() == filter.SearchValue) ||
                                        (x.JobStatus.ToString() == filter.SearchValue) ||
                                        (x.AddressId.ToString() == filter.SearchValue)
                                        || (x.JobName ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.ContactFirstName ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.ContactLasttName ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.JobType.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.JobTypeName ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.AssignedPersonnelName ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.JobStatus.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.AssignedPersonnel.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.Notes.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.Label ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.StreetAddress ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.State ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.ZipCode ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.City ?? "").ToLower().Contains(filter.SearchValue)))
                                    .ToList();

                    var TakenRecords = FilterRecords
                        .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                        .Take(filter.RecordPerPage)
                        .ToList();

                    responseVm.Data = TakenRecords;
                    responseVm.TotalRecords = FilterRecords.Count;
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                }
                else
                {
                    responseVm.Message = AppConstant.AccessDenied;
                }

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;

        }

        public async Task<IResponseVm<string>> Update(UpdateJob model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsTenantvalid = await tenantService.AnyAsync(x => x.IsActive == true && x.IsDelete == false
                && x.TenantId == AppConstant.LoggedInTenantId);
                if (!IsTenantvalid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsJobExist = (await GetAll()).Where(x => x.JobId == model.JobId && x.AddressId == model.AddressId &&
                x.ContactId == model.ContactId && x.JobUniqueId == model.JobUniqueCode.Trim() 
                && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false).FirstOrDefault();
                bool IsjobNameExists = await AnyAsync(x =>x.JobId !=model.JobId && x.JobName.Trim() == model.JobName.Trim() && 
                x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId && x.ContactId == model.ContactId);
                if (IsjobNameExists)
                {
                    responseVm.Message = "Job Name " + AppConstant.AlreadyExists;
                    return responseVm;
                }
                if (IsJobExist != null)
                {
                    string arrayString = string.Join(", ", model.AttachmentsAny);
                    IsJobExist.JobName = model.JobName?.Trim();
                    IsJobExist.JobType = model.JobType;
                    IsJobExist.AssignedPersonnel = model.AssignedPersonnel;
                    IsJobExist.ScheduledDate = model.ScheduledDate;
                    IsJobExist.JobStatus = model.JobStatus;
                    IsJobExist.Notes = model.Notes.Trim();
                    IsJobExist.AttachmentAny = arrayString;
                    IsJobExist.UpdatedAt = DateTime.Now;
                    IsJobExist.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = await Update(IsJobExist);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
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


        public async Task<IResponseVm<List<GetAllJobsSubtenant_tenant>>> GetAllJobsTenant_SubTenant(Pagination filter)
        {
            IResponseVm<List<GetAllJobsSubtenant_tenant>> responseVm = new ResponseVm<List<GetAllJobsSubtenant_tenant>>();
            try
            {
               
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true);

                var subTenantlist = (await tenantService.GetAll())
                  .Where(x => x.ParentTenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false)
                  .Select(pt => pt.TenantId)
                  .ToList(); 

                if (IsTenant)
                {
                    var JobList = await (from t2 in _context.TblJobs.OrderByDescending(x => x.CreatedAt)
                                         join t1 in _context.TblAddresses on t2.AddressId equals t1.AddressId
                                         join t3 in _context.MstJobTypes on t2.JobType equals t3.Id
                                         join t4 in _context.TblUsers on t2.AssignedPersonnel equals t4.Id
                                         join p1 in _context.TblUsers on t2.CreatedBy equals p1.Id
                                         join p2 in _context.TblTenants on t2.TenantId equals p2.TenantId
                                         join t5 in _context.MstJobStatuses on t2.JobStatus equals t5.JobStatusId
                                         join t6 in _context.TblContacts on t2.ContactId equals t6.ContactId
                                         where t6.IsDelete == false && t6.IsActive == true && t2.TenantId == AppConstant.LoggedInTenantId || subTenantlist.Contains((Guid)t2.TenantId)
                                         select new GetAllJobsSubtenant_tenant
                                         {
                                             IsActive = t2.IsActive,
                                             JobId = t2.JobId,
                                             JobName = t2.JobName,
                                             ContactId = t2.ContactId,
                                             ContactFirstName = t6.FirstName,
                                             ContactLastName = t6.LastName,
                                             ContactEmail = t6.Email,
                                             ContactPhone = t6.PhoneNumber,
                                             AddressId = t2.AddressId,
                                             JobType = t3.Id,
                                             JobTypeName = t3.JobTypeName,
                                             AssignedPersonnel = t4.Id,
                                             AssignedPersonnelName = t4.FirstName + " " + t4.LastName,
                                             ScheduledDate = t2.ScheduledDate,
                                             JobStatus = t5.JobStatusId,
                                             JobStatusName = t5.JobStatusName,
                                             Notes = t2.Notes,
                                             AttachmentsAny = t2.AttachmentAny,
                                             Label = t1.Label,
                                             StreetAddress = t1.StreetAddress,
                                             City = t1.City,
                                             State = t1.State,
                                             ZipCode = t1.ZipCode,
                                             JobUniqueCode = t2.JobUniqueId,
                                             CreatedBy = t2.CreatedBy.ToString(),
                                             CreatedbyUsername = p1.FirstName + " " + p1.LastName,
                                             SubtenantName = p2.FirstName + " " + p2.LastName,
                                             TenantId = t2.TenantId
                                         }).ToListAsync();

                   
                    filter.SearchValue = filter.SearchValue?.Trim().ToLower();

                    var FilterRecords = JobList
                        .Where(x =>
                            (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                            (string.IsNullOrEmpty(filter.JobStatus) ? true : (filter.JobStatus == x.JobStatus.ToString())) &&
                            (string.IsNullOrEmpty(filter.Address) ? true : (filter.Address == x.AddressId.ToString())) &&
                            (string.IsNullOrEmpty(filter.CustomerId) ? true : (filter.CustomerId == x.ContactId.ToString())) &&
                            (string.IsNullOrEmpty(filter.TenantId) ? true : (Guid.Parse(filter.TenantId) == x.TenantId)) &&
                            (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                (x.JobId.ToString() == filter.SearchValue) ||
                                (x.ContactId.ToString() == filter.SearchValue) ||
                                (x.JobStatus.ToString() == filter.SearchValue) ||
                                (x.AddressId.ToString() == filter.SearchValue)
                                || (x.JobName ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.ContactFirstName ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.ContactLastName ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.JobType.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.JobTypeName ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.AssignedPersonnelName ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.JobStatus.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.AssignedPersonnel.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.Notes.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.Label ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.StreetAddress ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.State ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.ZipCode ?? "").ToLower().Contains(filter.SearchValue)
                                || (x.City ?? "").ToLower().Contains(filter.SearchValue)))
                        .ToList();

                    var TakenRecords = Paginate.Pagination(FilterRecords, filter);

                    responseVm.Data = TakenRecords;
                    responseVm.TotalRecords = FilterRecords.Count();
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                }
                else
                {
                    responseVm.Message = AppConstant.AccessDenied;
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
