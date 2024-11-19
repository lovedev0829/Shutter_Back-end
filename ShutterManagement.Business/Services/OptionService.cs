using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class OptionService : BaseRepository<ShutterDbContext, TblOption>, IOption
    {
        private readonly ShutterDbContext _context;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;

        public OptionService(ShutterDbContext context, ClaimsProperty claimsProperty, ITenantService tenantService)
        {
            _context = context;
            this.claimsProperty = claimsProperty;
            this.tenantService = tenantService;
        }
        public async Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                if (IsTenant)
                {
                    var IsOptionExists = GetAllQuerable()
                       .Where(c => c.OptionId == Id
                                   && c.TenantId == AppConstant.LoggedInTenantId
                                   && c.IsActive == true
                                   && c.IsDelete == false)
                       .FirstOrDefault();
                    if (IsOptionExists != null)
                    {
                        IsOptionExists.IsActive = false;
                        IsOptionExists.UpdatedAt = DateTime.Now;
                        IsOptionExists.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = Update(IsOptionExists);
                        responseVm.IsSuccess = true;
                        responseVm.Message = $"{AppConstant.UpdateSuccessMsg}";
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoUserFound;
                    }
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Create_Option(CreateOption model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                if (IsTenant)
                {
                    foreach (var item in model.Options)
                    {
                        TblOption options = new TblOption
                        {
                            OptionId = Guid.NewGuid(),
                            OptionName = item.OptionName,
                            OptionType = model.OptionType,
                            AdditionalCost = item.AdditionalCost,
                            ProductTypeId = model.ProductTypeId,
                            CreatedAt = DateTime.Now,
                            CreatedBy = AppConstant.LoggedInUserID,
                            IsActive = true,
                            IsDelete = false
                        };
                        await Add(options);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.InsertSuccessMsg;
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                if (IsTenant)
                {
                    var IsOptionExists = GetAllQuerable()
                       .Where(c => c.OptionId == Id
                                   && c.TenantId == AppConstant.LoggedInTenantId
                                   && c.IsActive == true
                                   && c.IsDelete == false)
                       .FirstOrDefault();
                    if (IsOptionExists != null)
                    {
                        IsOptionExists.IsDelete = true;
                        IsOptionExists.IsActive = false;
                        IsOptionExists.UpdatedAt = DateTime.Now;
                        IsOptionExists.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = Update(IsOptionExists);
                        responseVm.IsSuccess = true;
                        responseVm.Message = $"{AppConstant.UpdateSuccessMsg}";
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoUserFound;
                    }
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public Task<IResponseVm<dynamic>> GetAll_Option(Pagination filter)
        {
            throw new NotImplementedException();
        }

        //public async Task<IResponseVm<dynamic>> GetAll_Option(Pagination filter)
        //{
        //    var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
        //    var IsSubTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == Guid.Parse(AppConstant.LoggedInParentTenantId));
        //    if (IsTenant || IsSubTenant) { }
        //}

        //public async Task<IResponseVm<dynamic>> GetAll_Option(Pagination filter)
        //{
        //    var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
        //    var IsSubTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == Guid.Parse(AppConstant.LoggedInParentTenantId));
        //    if (IsTenant || IsSubTenant) { } 


        //}



        //public async Task<IResponseVm<string>> Update_Option(UpdateOption model)
        //{
        //    IResponseVm<string> responseVm = new ResponseVm<string>();
        //    try
        //    {
        //        var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
        //        if (IsTenant)
        //        {
        //            var IsOptionExists = GetAllQuerable()
        //                .Where(c => c.OptionId == Guid.Parse(model.OptionId)
        //                            && c.TenantId == AppConstant.LoggedInTenantId
        //                            && c.IsActive == true
        //                            && c.IsDelete == false)
        //                .FirstOrDefault();
        //            if(IsOptionExists != null)
        //            {
        //                IsOptionExists.OptionName = model.OptionName;
        //                IsOptionExists.OptionType = model.OptionType;
        //                IsOptionExists.Description = model.Description;
        //                IsOptionExists.AdditionalCost = model.AdditionalCost;
        //                IsOptionExists.ProductTypeId = Guid.Parse(model.ProductTypeId);
        //                IsOptionExists.UpdatedAt = DateTime.Now;
        //                IsOptionExists.ModifiedBy = AppConstant.LoggedInUserID;
        //                var res = Update(IsOptionExists);
        //                responseVm.IsSuccess = true;
        //                responseVm.Message = $"{AppConstant.UpdateSuccessMsg}";
        //            }
        //             else
        //            {
        //                responseVm.Message = AppConstant.NoUserFound;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.Message = AppConstant.ExceptionError + ex.Message;
        //    }

        //    return responseVm;
        //}

        
    }
}
