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

        public MaterialService(ShutterDbContext context, ClaimsProperty claimsProperty)
        {
            _context = context;
            this.claimsProperty = claimsProperty;
        }

        public Task<IResponseVm<string>> ActiveInActive(Guid Id)
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
                var IsTenantExists = _context.TblTenants.Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true);
                if (IsTenantExists != null)
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

        public async Task<IResponseVm<dynamic>> GetAll_Material(Pagination filter)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var IsMaterialExists = (await GetAll())
                    .Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId)
                    .ToList();

                var FilterRecords = IsMaterialExists
                .Where(x => string.IsNullOrEmpty(filter.SearchValue) ||
                 (x.MaterialName ?? "").Contains(filter.SearchValue))
                .ToList();

                //(x.VendorId ?? "").Contains(filter.SearchValue)) &&  
                //(string.IsNullOrEmpty(filter.VendorId) ||
                //  x.TenantId.ToString() == filter.VendorId))  
                //  .ToList();


                var TakenRecords = FilterRecords
                    .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                    .Take(filter.RecordPerPage)
                    .ToList();

                responseVm.IsSuccess = true;
                responseVm.Data = TakenRecords; 
                responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return responseVm;  
        }


        public Task<IResponseVm<string>> Update_Material(UpdateMaterial model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                if (Guid.TryParse(model.RawMaterialId, out Guid rawMaterialIdGuid))
                {
                    var IsMaterialExists = GetAllQuerable()
                        .Where(c => c.RawMaterialId == rawMaterialIdGuid
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
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return Task.FromResult(responseVm);
        }
    }
}
