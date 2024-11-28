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
using static Azure.Core.HttpHeader;

namespace ShutterManagement.Business.Services
{
    public class JobService : BaseRepository<ShutterDbContext, TblJob>, IJobService
    {
        private readonly ShutterDbContext _context;
        private readonly ClaimsProperty claimsProperty;
        private readonly IAddress address;
        private readonly ITenantService tenantService;

        public JobService(ShutterDbContext context,ClaimsProperty claimsProperty,IAddress address,ITenantService tenantService)
        {
            _context = context;
            this.claimsProperty = claimsProperty;
            this.address = address;
            this.tenantService = tenantService;
        }
        public Task<IResponseVm<string>> ActiveInactive(string UserId)
        {
            throw new NotImplementedException();
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

        public Task<IResponseVm<string>> Delete(string UserId)
        {
            throw new NotImplementedException();
        }

        public Task<IResponseVm<List<UsersList>>> GetAll(Pagination filter)
        {
            throw new NotImplementedException();
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
              var IsAddressExist = await GetAll(x =>x.JobId == model.JobId && x.AddressId == model.AddressId && x.ContactId == model.ContactId
              && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false);

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
    }
}
