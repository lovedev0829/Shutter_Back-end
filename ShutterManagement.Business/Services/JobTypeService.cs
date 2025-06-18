using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Services;
using ShutterManagement.DAL.Context;
using ShutterManagement.Business.Interfaces;

namespace ShutterManagement.Business.Services
{
    public class JobTypeService : BaseRepository<ShutterDBContext, MstJobType>, IJobTypeService
    {
        private readonly ITenantService tenantService;

        public JobTypeService(ITenantService tenantService)
        {
            this.tenantService = tenantService;
        }
        public async Task<IResponseVm<List<JobTypeList>>> GetAllJobType(Pagination filter)
        {
            IResponseVm<List<JobTypeList>> responseVm = new ResponseVm<List<JobTypeList>>();
            try
            {
                var isTenantValid = await tenantService.AnyAsync(x =>
                x.TenantId == claimsProperty.GetTenantId() &&
                //  x.ParentTenantId == null &&
                x.IsDelete == false &&
                x.IsActive == true);
                if (isTenantValid)
                {

                    var JobTypeList = (await GetAll()).Where(p => p.IsDelete == false && (p.TenantId.ToString().ToUpper() == claimsProperty.GetTenantId().ToString().ToUpper() || p.TenantId.ToString().ToUpper() == claimsProperty.GetParentTenantId().ToUpper())).OrderByDescending(x => x.CreatedAt).ToList();


                    filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                    var FilterRecords = JobTypeList
                              .Where(x =>
                              (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                              (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                 (x.Id.ToString() == filter.SearchValue) ||
                                 (x.JobTypeName ?? "").ToLower().Contains(filter.SearchValue)
                                )
                             ).Select(x => new JobTypeList
                             {
                                 JobTypeId = x.Id,
                                 JobTypeName = x.JobTypeName,
                                 IsActive = x.IsActive
                             }).ToList();



                    var TakenRecords = FilterRecords
                            .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                            .Take(filter.RecordPerPage)
                            .ToList();
                    responseVm.IsSuccess = true;

                    if (TakenRecords != null && TakenRecords.Count > 0)
                    {
                        responseVm.Data = TakenRecords.ToList();
                        responseVm.Message = AppConstant.FetchSuccessMsg;
                        responseVm.TotalRecords = FilterRecords.Count;
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<string>> ActiveInActive(Guid? JobTypeId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (!IsTenant)
                {
                    responseVm.Message = "Tenant not found";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var IsProductTypeExists = GetAllQuerable()
                      .Where(c => c.Id == JobTypeId
                                  && c.IsDelete == false  && c.TenantId == claimsProperty.GetTenantId())
                      .FirstOrDefault();
                if (IsProductTypeExists != null)
                {
                    IsProductTypeExists.IsActive = !IsProductTypeExists.IsActive;
                    IsProductTypeExists.UpdatedAt = DateTime.Now;
                    IsProductTypeExists.ModifiedBy = AppConstant.LoggedInUserID;

                    var res = Update(IsProductTypeExists);

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

        public async Task<IResponseVm<string>> Create_JobType(CreateJobType model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (!IsTenant)
                {
                    responseVm.Message = "Tenant not found";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }
                var existingProductType = GetAllQuerable()
                    .FirstOrDefault(x => x.JobTypeName == model.JobTypeName && x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId());

                if (existingProductType != null)
                {
                    responseVm.Message = $"ProductTypeName {AppConstant.AlreadyExists}";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

               


                var productData = new MstJobType
                {
                    Id = Guid.NewGuid(),
                    JobTypeName = model.JobTypeName,
                    IsActive = true,
                    IsDelete = false,
                    CreatedAt = DateTime.Now,
                    TenantId = claimsProperty.GetTenantId(),
                    CreatedBy = AppConstant.LoggedInUserID,
                };

                var result = Add(productData);
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

        public async Task<IResponseVm<string>> DeleteJobType(Guid? JobTypeId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (!IsTenant)
                {
                    responseVm.Message = "Tenant not found";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var IsProductTypeExists = GetAllQuerable()
                      .Where(c => c.Id == JobTypeId
                                  && c.IsDelete == false)
                      .FirstOrDefault();
                if (IsProductTypeExists != null)
                {
                    IsProductTypeExists.IsDelete = true;
                    IsProductTypeExists.IsActive = false;
                    IsProductTypeExists.UpdatedAt = DateTime.Now;
                    IsProductTypeExists.ModifiedBy = AppConstant.LoggedInUserID;

                    var res = Update(IsProductTypeExists);

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


        public async Task<IResponseVm<string>> Update_JobType(UpdateJobType model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {

                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (!IsTenant)
                {
                    responseVm.Message = "Tenant not found";
                    responseVm.IsSuccess = false;
                    return responseVm;
                }

                var IsProductTypeExists = GetAllQuerable()
                      .Where(c => c.Id == model.JobTypeId
                                  && c.IsDelete == false && c.TenantId == claimsProperty.GetTenantId())
                      .FirstOrDefault();

               

                if (IsProductTypeExists != null)
                {
                    IsProductTypeExists.JobTypeName = model.JobTypeName;
                    IsProductTypeExists.UpdatedAt = DateTime.Now;
                    IsProductTypeExists.ModifiedBy = AppConstant.LoggedInUserID;
                    var res = Update(IsProductTypeExists);

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
