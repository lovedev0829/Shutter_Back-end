using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
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
using static Azure.Core.HttpHeader;
using ShutterManagement.Core.Common;

namespace ShutterManagement.Business.Services
{
    public class JobService : BaseRepository<ShutterDbContext, TblJob>, IJobService
    {
        private readonly ShutterDbContext _context;
        private readonly ClaimsProperty claimsProperty;
        private readonly IAddress address;
        private readonly ITenantService tenantService;

        public JobService(ShutterDbContext context,ClaimsProperty claimsProperty,IAddress address,
            ITenantService tenantService)
        {
            _context = context;
            this.claimsProperty = claimsProperty;
            this.address = address;
            this.tenantService = tenantService;
        }
        public  async Task<IResponseVm<string>> ActiveInactive(string Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsTenantvalid = await AnyAsync(x => x.IsActive == true && x.IsDelete == false
                && x.TenantId == AppConstant.LoggedInTenantId);
                if (!IsTenantvalid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsAddressExist = (await GetAll()).Where(x => x.JobId.ToString() == Id &&
                  x.IsDelete == false).FirstOrDefault();
                if (IsAddressExist != null)
                {
                    IsAddressExist.IsActive = !IsAddressExist.IsActive;
                    IsAddressExist.UpdatedAt = DateTime.Now;
                    IsAddressExist.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = await Update(IsAddressExist);
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
                GenerateUniqueId generateUnique = new GenerateUniqueId(_context);
                bool IsAddressExist = await address.AnyAsync(x => x.AddressId == model.AddressId && x.ContactId == model.ContactId 
                && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false);
                if (IsAddressExist) 
                {
                    TblJob job = new TblJob
                    {
                        AddressId = model.AddressId,
                        ContactId = model.ContactId,
                        JobName = model.JobName,
                        JobType = model.JobType,
                        AssignedPersonnel = model.AssignedPersonnel,
                        ScheduledDate = model.ScheduledDate,
                        JobStatus = model.JobStatus,
                        Notes = model.Notes,
                        AttachmentAny = model.AttachmentsAny,
                        JobUniqueId = generateUnique.GenerateUniqueIds(ModuleName.Job),
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
                    responseVm.Message = AppConstant.NoDataFound;
                }


            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> Delete(string Id)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                bool IsTenantvalid = await AnyAsync(x => x.IsActive == true && x.IsDelete == false
                && x.TenantId == AppConstant.LoggedInTenantId);
                if (!IsTenantvalid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                var IsAddressExist = (await GetAll()).Where(x => x.JobId.ToString() == Id &&
                  x.IsDelete == false).FirstOrDefault();
                if (IsAddressExist != null)
                {
                    IsAddressExist.IsActive = !IsAddressExist.IsActive;
                    IsAddressExist.IsDelete = false;
                    IsAddressExist.UpdatedAt = DateTime.Now;
                    IsAddressExist.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = await Update(IsAddressExist);
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

        public async Task<IResponseVm<List<GetAllJobsdetails>>> GetAll(Pagination filter)
        {

            IResponseVm<List<GetAllJobsdetails>> responseVm = new ResponseVm<List<GetAllJobsdetails>>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true);

                if (IsTenant)
                {

                    var JobList = (from t2 in _context.TblJobs
                                   join t1 in _context.TblAddresses on t2.AddressId equals t1.AddressId
                                        where t2.TenantId == AppConstant.LoggedInTenantId && t2.IsDelete == false
                                    select new GetAllJobsdetails
                                    {
                                       IsActive = t2.IsActive,
                                       JobId = t2.JobId,
                                       JobName = t2.JobName,
                                       ContactId = t2.ContactId,
                                       AddressId = t2.AddressId,
                                       JobType = t2.JobType,
                                       AssignedPersonnel = t2.AssignedPersonnel,
                                       ScheduledDate = t2.ScheduledDate,
                                       JobStatus = t2.JobStatus,
                                       Notes = t2.Notes,
                                       AttachmentsAny = t2.AttachmentAny,
                                       Label = t1.Label,
                                       StreetAddress = t1.StreetAddress,
                                       City= t1.City,
                                       State=t1.State,
                                       ZipCode = t1.ZipCode                                    
                                   }).ToList();
                    filter.SearchValue = filter.SearchValue.Trim().ToLower();
                    var FilterRecords = JobList
                                    .Where(x =>
                                     (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                        (x.JobId.ToString() == filter.SearchValue) ||
                                        (x.ContactId.ToString() == filter.SearchValue) ||
                                        (x.AddressId.ToString() == filter.SearchValue) 
                                        || (x.JobName ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.JobType.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.JobStatus.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.JobName ?? "").ToLower().Contains(filter.SearchValue)
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

                    responseVm.Data = TakenRecords.Select(x => new GetAllJobsdetails
                    {
                        IsActive = x.IsActive,
                        JobId = x.JobId,
                        JobName = x.JobName,
                        ContactId = x.ContactId,
                        AddressId = x.AddressId,
                        JobType = x.JobType,
                        AssignedPersonnel = x.AssignedPersonnel,
                        ScheduledDate = x.ScheduledDate,
                        JobStatus = x.JobStatus,
                        Notes = x.Notes,
                        AttachmentsAny = x.AttachmentsAny,
                        Label = x.Label,
                        StreetAddress = x.StreetAddress,
                        City = x.City,
                        State = x.State,
                        ZipCode = x.ZipCode
                    }).ToList();
                    responseVm.TotalRecords = FilterRecords.Count;
                    responseVm.Data = TakenRecords;
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.TotalRecords = FilterRecords.Count;
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
                bool IsTenantvalid = await AnyAsync(x => x.IsActive == true && x.IsDelete == false
                && x.TenantId == AppConstant.LoggedInTenantId );
                if (!IsTenantvalid)
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
              var IsAddressExist = (await GetAll()).Where(x =>x.JobId == model.JobId && x.AddressId == model.AddressId && x.ContactId == model.ContactId
              && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false).FirstOrDefault();
                if (IsAddressExist != null)
                {
                    IsAddressExist.AddressId = model.AddressId;
                    IsAddressExist.JobName = model.JobName;
                    IsAddressExist.JobType = model.JobType;
                    IsAddressExist.AssignedPersonnel = model.AssignedPersonnel;
                    IsAddressExist.ScheduledDate = model.ScheduledDate;
                    IsAddressExist.JobStatus = model.JobStatus;
                    IsAddressExist.Notes = model.Notes;
                    IsAddressExist.AttachmentAny = model.AttachmentsAny;
                    IsAddressExist.UpdatedAt = DateTime.Now;
                    IsAddressExist.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = await Update(IsAddressExist);
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
    }
}
