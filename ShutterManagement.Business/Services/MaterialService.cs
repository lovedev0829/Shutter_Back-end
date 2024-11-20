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
    public class MaterialService : BaseRepository<ShutterDbContext, TblRawMaterial>, IMaterial
    {
        private readonly ShutterDbContext _context;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;

        public MaterialService(ShutterDbContext context, ClaimsProperty claimsProperty,ITenantService tenantService)
        {
            _context = context;
            this.claimsProperty = claimsProperty;
            this.tenantService = tenantService;
        }

        public Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant =  tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                var IsMaterialExists = GetAllQuerable()
                      .Where(c => c.RawMaterialId == Id
                                  && c.TenantId == AppConstant.LoggedInTenantId
                                  && c.IsActive == true
                                  && c.IsDelete == false)
                      .FirstOrDefault();
                if (IsMaterialExists != null)
                {
                    IsMaterialExists.IsActive = false;
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
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return Task.FromResult(responseVm);
        }

        public async Task<IResponseVm<string>> Create_Material(CreateMaterial model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                var IsTenantExists = _context.TblTenants.Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true);
                var IsVendorExist = _context.TblVendors.Where(x => x.VendorId == model.VendorId && x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                if (IsTenantExists != null)
                {
                    if (IsVendorExist != null)
                    {

                        TblRawMaterial material = new TblRawMaterial
                        {
                            MaterialName = model.MaterialName,
                            UnitCost = model.UnitCost,
                            UnitOfMeasurement = model.UnitOfMeasurement,
                            StockQuantity = model.StockQuantity,
                            ReorderLevel = model.ReorderLevel,
                            VendorId = model.VendorId,
                            Notes = model.Notes,
                            CreatedAt = DateTime.Now,
                            CreatedBy = AppConstant.LoggedInUserID,
                            IsActive = true,
                            IsDelete = false,
                            TenantId = AppConstant.LoggedInTenantId
                        };
                        if (material != null)
                        {
                            try
                            {
                                await Add(material);
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
                        responseVm.IsSuccess = false;
                        responseVm.Message = "Vendor does not exist";
                    }
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public Task<IResponseVm<string>> Delete(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsMaterialExists = GetAllQuerable()
                      .Where(c => c.RawMaterialId == Id
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
            catch(Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return Task.FromResult(responseVm);

        }

        public async Task<IResponseVm<List<GetAllMaterialVm>>> GetAll_Material(Pagination filter)
        {
            IResponseVm<List<GetAllMaterialVm>> responseVm = new ResponseVm<List<GetAllMaterialVm>>();
            try
            {
                var IsMaterialExists = (await GetAll())
                    .Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId)
                    .ToList();

                var FilterRecords = IsMaterialExists
                .Where(x => string.IsNullOrEmpty(filter.SearchValue.Trim()) ||
                 (x.MaterialName ?? "").Contains(filter.SearchValue.Trim()))
                .ToList();
                var TakenRecords = FilterRecords
                    .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                    .Take(filter.RecordPerPage)
                    .ToList();



                responseVm.IsSuccess = true;
                responseVm.Data = (List<GetAllMaterialVm>)TakenRecords.Select( x => new
                {
                    x.MaterialName,
                    x.IsActive,
                    x.VendorId,
                    x.UnitOfMeasurement,
                    
                    x.StockQuantity,
                    x.ReorderLevel
                });
                responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return responseVm;  
        }


        public async Task<IResponseVm<string>> Update_Material(UpdateMaterial model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                var IsVendorExist = _context.TblVendors.Where(x => x.VendorId == model.VendorId && x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                if (IsTenant)
                {
                    if (IsVendorExist != null)
                    {
                        var IsMaterialExists = GetAllQuerable()
                                 .Where(c => c.RawMaterialId == Guid.Parse(model.RawMaterialId)
                                               && c.TenantId == AppConstant.LoggedInTenantId
                                               && c.IsActive == true
                                               && c.IsDelete == false)
                                   .FirstOrDefault();
                        if (IsMaterialExists != null)
                        {
                            IsMaterialExists.MaterialName = model.MaterialName;
                            IsMaterialExists.UnitCost = model.UnitCost;
                            IsMaterialExists.UnitOfMeasurement = model.UnitOfMeasurement;
                            IsMaterialExists.StockQuantity = model.StockQuantity;
                            IsMaterialExists.ReorderLevel = model.ReorderLevel;
                            IsMaterialExists.VendorId = model.VendorId;
                            IsMaterialExists.Notes = model.Notes;
                            IsMaterialExists.ModifiedBy = AppConstant.LoggedInUserID;
                            IsMaterialExists.UpdatedAt = DateTime.Now;
                            var res = Update(IsMaterialExists);
                            responseVm.IsSuccess = true;
                            responseVm.Message = $"{AppConstant.UpdateSuccessMsg}";
                        }
                        else
                        {
                            responseVm.Message = AppConstant.NoUserFound;
                        }
                    
                    }
                    else
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = "Vendor does not exist";
                    }
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return (responseVm);
        }
    }
}
