using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace ShutterManagement.Business.Services
{
    public class ComponentService : BaseRepository<ShutterDbContext, TblComponent>, IComponentService
    {
        private readonly ShutterDbContext _dbContext;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;

        public ComponentService(ShutterDbContext dbContext, ClaimsProperty claimsProperty,ITenantService tenantService)
        {
            _dbContext = dbContext;
            this.claimsProperty = claimsProperty;
            this.tenantService = tenantService;
        }


        public Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant =  tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                var IsComponentExists = GetAllQuerable()
                      .Where(c => c.ComponentId == Id
                                  && c.TenantId == AppConstant.LoggedInTenantId
                                  && c.IsActive == true
                                  && c.IsDelete == false)
                      .FirstOrDefault();
                if (IsComponentExists != null)
                {
                    IsComponentExists.IsActive = false;
                    IsComponentExists.UpdatedAt = DateTime.Now;
                    IsComponentExists.ModifiedBy = AppConstant.LoggedInUserID;

                    var res = Update(IsComponentExists);

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.DeleteSuccessMsg;
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

            return Task.FromResult(responseVm);
        }

        public async Task<IResponseVm<string>> Create_Component(CreateComponent model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (IsTenant)
                {
                    TblComponent component = new TblComponent
                    {

                        ComponentName = model.ComponentName,
                        ProductType = model.ProductType,
                        PricingExpression = model.PricingExpression,
                        MarkupPercentage = model.MarkupPercentage,
                        Notes = model.Notes,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID,
                        IsActive = true,
                        IsDelete = false,
                        TenantId = AppConstant.LoggedInTenantId
                    };


                    if (component != null)
                    {
                        try
                        {
                            Add(component);
                            responseVm.IsSuccess = true;
                            responseVm.Message = $"{AppConstant.InsertSuccessMsg}";
                        }
                        catch (Exception ex)
                        {
                            responseVm.Message = AppConstant.ExceptionError + ex.Message;
                        }
                    }
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

        public async Task<IResponseVm<string>> Delete(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false );
                if (IsTenant)
                {
                    var IsMaterialExists = GetAllQuerable()
                          .Where(c => c.ComponentId == Id
                                      && c.TenantId == AppConstant.LoggedInTenantId
                                      && c.IsActive == true
                                      && c.IsDelete == false)
                          .FirstOrDefault();
                    if (IsMaterialExists != null)
                    {
                        IsMaterialExists.IsActive = false;
                        IsMaterialExists.IsDelete = true;
                        IsMaterialExists.UpdatedAt = DateTime.Now;
                        IsMaterialExists.ModifiedBy = AppConstant.LoggedInUserID;

                        var res = Update(IsMaterialExists);

                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.DeleteSuccessMsg;
                    }
                    else
                    {
                        responseVm.Message = $"{AppConstant.NoDataFound}";
                    }

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

        public async Task<IResponseVm<List<ComponentListVm>>> GetAll_Component(Pagination filter)
        {
            IResponseVm<List<ComponentListVm>> responseVm = new ResponseVm<List<ComponentListVm>>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);

                if (IsTenant)
                {
                    var IsComponentExists = (await GetAll())
                        .Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId)
                        .ToList();

                    var FilterRecords = IsComponentExists
                        .Where(x => string.IsNullOrEmpty(filter.SearchValue) ||
                                    (x.ComponentName ?? "").Contains(filter.SearchValue.Trim()) ||
                                    (x.ProductType ?? "").Contains(filter.SearchValue.Trim()) ||
                                    (x.PricingExpression ?? "").Contains(filter.SearchValue.Trim()))
                        .ToList();

                    var TakenRecords = FilterRecords
                        .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                        .Take(filter.RecordPerPage)
                        .ToList();


                    responseVm.IsSuccess = true;
                    responseVm.Data = (List<ComponentListVm>)TakenRecords.Select(x => new
                    {
                        x.ComponentId,
                        x.IsActive,
                        x.ComponentName,
                        x.TenantId,
                       x.MarkupPercentage,
                       x.PricingExpression,
                       x.Notes
                    });
                    responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
                }
                else
                {
                    responseVm.Message = AppConstant.AccessDenied;
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return await Task.FromResult(responseVm);
        }


        public async Task<IResponseVm<dynamic>> Update_Component(UpdateComponent model)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);

                if (IsTenant) 
                { 
              
                    var IsComponentExists = GetAllQuerable()
                       .Where(c => c.ComponentId == Guid.Parse(model.ComponentId)
                                   && c.TenantId == AppConstant.LoggedInTenantId
                                   && c.IsDelete == false)
                       .FirstOrDefault();
                    if(IsComponentExists != null)
                    {
                        IsComponentExists.ComponentName = model.ComponentName;
                        IsComponentExists.ProductType = model.ProductType;
                        IsComponentExists.PricingExpression = model.PricingExpression;
                        IsComponentExists.MarkupPercentage = model.MarkupPercentage;
                        IsComponentExists.Notes = model.Notes;
                        IsComponentExists.UpdatedAt = DateTime.Now;
                        IsComponentExists.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = Update(IsComponentExists);
                        responseVm.IsSuccess = true;
                        responseVm.Message = $"{AppConstant.UpdateSuccessMsg}";
                    }
                    else
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = $"{AppConstant.NoDataFound}";
                    }
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
