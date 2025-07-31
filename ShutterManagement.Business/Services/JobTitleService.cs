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
    public class JobTitleService : BaseRepository<ShutterDBContext, MstJobTitle>, IJobTitleService
    {
        private readonly ITenantService tenantService;
        private readonly ClaimsProperty claimsProperty;

        public JobTitleService(ITenantService tenantService, ClaimsProperty claimsProperty)
        {
            this.tenantService = tenantService;
            this.claimsProperty = claimsProperty;
        }
        public async Task<IResponseVm<List<JobTitlesList>>> GetAll(Pagination filter)
        {
            IResponseVm<List<JobTitlesList>> responseVm = new ResponseVm<List<JobTitlesList>>();
            try
            {
                var TenantId = claimsProperty.GetTenantId();
                var subTenantIds = new List<Guid>();
                if (string.IsNullOrEmpty(claimsProperty.GetParentTenantId()))
                {
                    subTenantIds = tenantService.GetAccessibleTenantIds(TenantId);
                }

                var JobTitleList = (await GetAll()).Where(x => x.IsDelete == false && (x.TenantId == TenantId || subTenantIds.Contains(x.TenantId ?? Guid.Empty))).OrderByDescending(x => x.CreatedAt).ToList();

                filter.SearchValue = (filter.SearchValue?.Trim() ?? "").ToLower();
                var FilterRecords = JobTitleList
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (filter.SearchValue != "" ?
                                    (x.Id.ToString() == filter.SearchValue) ||
                                    ((x.JobTitleName ?? "").ToLower().Contains(filter.SearchValue)
                                    ) : true)).ToList();
                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();

                if (TakenRecords != null && TakenRecords.Count > 0)
                {
                    responseVm.Data = TakenRecords.Select(x => new JobTitlesList
                    {
                        Id = x.Id,
                        JobTitleName = x.JobTitleName,
                        IsActive = x.IsActive,
                    }).ToList();
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                    responseVm.TotalRecords = FilterRecords.Count;
                }
                else
                {
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.NoDataFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> ActiveInActive(Guid? JobTitleId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId()  && x.IsDelete == false && x.IsActive == true);
                if (!IsTenant)
                {
                    responseVm.Message = "Tenant not found";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var IsJobTitleExists = GetAllQuerable()
                      .Where(c => c.Id == JobTitleId
                                  && c.IsDelete == false && c.TenantId == claimsProperty.GetTenantId())
                      .FirstOrDefault();
                if (IsJobTitleExists != null)
                {
                    IsJobTitleExists.IsActive = !IsJobTitleExists.IsActive;
                    IsJobTitleExists.ModifiedAt = DateTime.Now;
                    IsJobTitleExists.ModifiedBy = AppConstant.LoggedInUserID;

                    var res = Update(IsJobTitleExists);

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
                else
                {
                    responseVm.Message = $"{AppConstant.NoDataFound}";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Create_Title(CreateJobTitle model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsDelete == false && x.IsActive == true);
                if (!IsTenant)
                {
                    responseVm.Message = "Tenant not found";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }
                var existingJobTitle = GetAllQuerable()
                    .FirstOrDefault(x => x.JobTitleName == model.JobTitleName && x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId());

                if (existingJobTitle != null)
                {
                    responseVm.Message = $"JobTitleName {AppConstant.AlreadyExists}";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }




                var JobTitleData = new MstJobTitle
                {
                    Id = Guid.NewGuid(),
                    JobTitleName = model.JobTitleName,
                    IsActive = true,
                    IsDelete = false,
                    CreatedAt = DateTime.Now,
                    TenantId = claimsProperty.GetTenantId(),
                    CreatedBy = AppConstant.LoggedInUserID,
                };

                var result = Add(JobTitleData);
                responseVm.Message = AppConstant.InsertSuccessMsg;
                responseVm.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseVm.Message = $"{AppConstant.ExceptionError} {ex.Message}";
                responseVm.IsSuccess = false;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> DeleteTitle(Guid? JobTitleId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsDelete == false && x.IsActive == true);
                if (!IsTenant)
                {
                    responseVm.Message = "Tenant not found";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var IsJobTitleeExists = GetAllQuerable().Where(c => c.Id == JobTitleId
                                  && c.IsDelete == false)
                      .FirstOrDefault();
                if (IsJobTitleeExists != null)
                {
                    IsJobTitleeExists.IsDelete = true;
                    IsJobTitleeExists.IsActive = false;
                    IsJobTitleeExists.ModifiedAt = DateTime.Now;
                    IsJobTitleeExists.ModifiedBy = AppConstant.LoggedInUserID;

                    var res = Update(IsJobTitleeExists);

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
                else
                {
                    responseVm.Message = $"{AppConstant.NoDataFound}";
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }


        public async Task<IResponseVm<string>> Update_Title(UpdateJobTitle model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {

                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId()  && x.IsDelete == false && x.IsActive == true);
                if (!IsTenant)
                {
                    responseVm.Message = "Tenant not found";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var IsJobTitleExists = GetAllQuerable().Where(c => c.Id == model.Id && c.IsDelete == false && c.TenantId == claimsProperty.GetTenantId()).FirstOrDefault();

                if (IsJobTitleExists != null)
                {
                    IsJobTitleExists.JobTitleName = model.JobTitleName;
                    IsJobTitleExists.ModifiedAt = DateTime.Now;
                    IsJobTitleExists.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = Update(IsJobTitleExists);

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
                else
                {
                    responseVm.Message = $"{AppConstant.NoDataFound}";
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

