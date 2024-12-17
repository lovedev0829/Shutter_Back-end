using Azure;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace ShutterManagement.Business.Services
{
    public class MaterialService : BaseRepository<BtprojecShuttermanagementDevContext, TblRawMaterial>, IMaterial
    {
        private readonly BtprojecShuttermanagementDevContext _context;
        private readonly IVendorService vendorService;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;
        private readonly IComponentMaterial componentMaterial;
        private readonly IUnitofMeasurementService unitofMeasurementService;

        public MaterialService(BtprojecShuttermanagementDevContext context, IVendorService vendorService,
            ClaimsProperty claimsProperty, ITenantService tenantService,IComponentMaterial componentMaterial,IUnitofMeasurementService unitofMeasurementService)
        {
            _context = context;
            this.vendorService = vendorService;
            this.claimsProperty = claimsProperty;
            this.tenantService = tenantService;
            this.componentMaterial = componentMaterial;
            this.unitofMeasurementService = unitofMeasurementService;
        }

        public async Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (IsTenant)
                {
                    var IsMaterialExists = GetAllQuerable()
                          .Where(c => c.RawMaterialId == Id
                                      && c.TenantId == AppConstant.LoggedInTenantId
                                      && c.IsDelete == false)
                          .FirstOrDefault();
                    if (IsMaterialExists != null)
                    {
                        IsMaterialExists.IsActive = !IsMaterialExists.IsActive;
                        IsMaterialExists.UpdatedAt = DateTime.Now;
                        IsMaterialExists.ModifiedBy = AppConstant.LoggedInUserID;

                        await Update(IsMaterialExists);

                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
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
        public async Task<IResponseVm<string>> Validate_MaterialExcell(ImportMaterial model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
               
                if(IsTenant)
                {
                    var VendorExit = vendorService.GetAllQuerable().Where(v => v.VendorCode == model.VendorCode && v.IsDelete == false)
                                   .FirstOrDefault();
                    var IsSKUExist = _context.TblRawMaterials.Any(x => x.VendorId == VendorExit.VendorId && x.Sku == model.SKU.Trim() && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false);
                    var GetAllVendorList = vendorService.GetAllQuerable()
                    .Where(x => x.IsActive == true && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).ToList();
                    var GetAllUnitOfMeasureList = unitofMeasurementService.GetAllQuerable()
                    .Where(x => x.IsActive == true && x.IsDelete == false).ToList();
                    var CheckExistingVendorbyCode = GetAllVendorList.Any(x => x.VendorCode == model.VendorCode);
                    var CheckExistingUnitOfMeasurebyCode = GetAllUnitOfMeasureList.Any(x => x.UnitofMeasurementName == model.UnitOfMeasurement);
                    if (IsSKUExist)
                    {
                        responseVm.Message = $"SKU {model.SKU}" + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    if (!CheckExistingVendorbyCode || !CheckExistingUnitOfMeasurebyCode)
                    {
                        responseVm.Message = $"Vendor Code {model.VendorCode} " +
                            $"or Unit of Measurement {model.UnitOfMeasurement} is Not Valid";
                    }
                    
                   
                }
                else
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                    return responseVm;
                }
                

            }
            catch(Exception ex) 
            {

            }
            
            return responseVm;
        }

        public async Task<IResponseVm<string>> Create_Material(CreateMaterial model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
                var IsVendorExist = await vendorService.AnyAsync(x => x.VendorId == model.VendorId && x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                var IsSKUExist = _context.TblRawMaterials.Any(x => x.VendorId == model.VendorId && x.Sku == model.SKU.Trim() && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false);
                if (IsSKUExist)
                {
                    responseVm.Message = $"SKU {model.SKU}" + AppConstant.AlreadyExists;
                    return responseVm;
                }
               

                if (IsTenant)
                {
                    if (IsVendorExist)
                    {

                        TblRawMaterial material = new TblRawMaterial
                        {
                            MaterialName = model.MaterialName.Trim(),
                            UnitCost = model.UnitCost,
                            UnitOfMeasurement = model.UnitOfMeasurement,
                            StockQuantity = model.StockQuantity,
                            ReorderLevel = model.ReorderLevel,
                            ProductType = Guid.Parse(model.ProductType),
                            Sku = model.SKU.Trim(),
                            IsCutSheet = model.IsCutSheet,
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
                        responseVm.Message = "Vendor does not exist";
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (IsTenant)
                {
                    var IsMaterialExists =  GetAllQuerable()
                          .Where(c => c.RawMaterialId == Id
                                      && c.TenantId == AppConstant.LoggedInTenantId
                                      && c.IsDelete == false && c.IsActive==true
                                      )
                          .FirstOrDefault();
                    var IsMaterialExistInComponent = componentMaterial.AnyAsync(x => x.RawMaterialId == Id && x.IsActive == true && x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId);
                    if (IsMaterialExistInComponent != null)
                    {
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
                        responseVm.Message = "Material exists in Component";
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

        public async Task<IResponseVm<List<GetAllMaterialVm>>> GetAll_Material(Pagination filter)
        {
            IResponseVm<List<GetAllMaterialVm>> responseVm = new ResponseVm<List<GetAllMaterialVm>>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true);

                if (IsTenant)
                {

                    var MaterialList = (from t2 in _context.TblRawMaterials.OrderByDescending(x => x.CreatedAt)
                                        join t1 in _context.TblVendors on t2.VendorId equals t1.VendorId
                                        where t2.TenantId == AppConstant.LoggedInTenantId && t2.IsDelete == false
                                        select new GetAllMaterialVm
                                        {
                                            IsActive = t2.IsActive,
                                            MaterialId = t2.RawMaterialId,
                                            MaterialName = t2.MaterialName,
                                            Notes = t2.Notes,
                                            ReorderLevel = t2.ReorderLevel,
                                            SKU = t2.Sku,
                                            StockQuantity = t2.StockQuantity,
                                            UnitCost = t2.UnitCost,
                                            ProductType = t2.ProductType.Value,
                                            UnitOfMeasurement = t2.UnitOfMeasurement,
                                            VendorId = t2.VendorId,
                                            VendorName = t1.VendorName,
                                            IsCutSheet = t2.IsCutSheet,
                                        }).ToList();
                    filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                    var FilterRecords = MaterialList
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (string.IsNullOrEmpty(filter.Vendor) ? true : (filter.Vendor == x.VendorId.ToString())) &&  
                                    (string.IsNullOrEmpty(filter.ProductType) ? true : (filter.ProductType == x.ProductType.ToString())) &&  
                                     (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                        (x.MaterialId.ToString() == filter.SearchValue)
                                        || (x.ProductType.ToString() == filter.SearchValue)
                                        || (x.MaterialName ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.UnitCost.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.ReorderLevel.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.StockQuantity.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.Notes.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.SKU ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.VendorName ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.UnitOfMeasurement ?? "").ToLower().Contains(filter.SearchValue)))
                                    .ToList();

                    var TakenRecords = FilterRecords
                        .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                        .Take(filter.RecordPerPage)
                        .ToList();

                    responseVm.Data = TakenRecords.Select(x => new GetAllMaterialVm
                    {
                        MaterialName = x.MaterialName,
                        IsActive = x.IsActive,
                        VendorId = x.VendorId,
                        TenantId=x.TenantId,
                        Notes = x.Notes,
                        UnitCost= x.UnitCost,
                        UnitOfMeasurement = x.UnitOfMeasurement,
                        StockQuantity = x.StockQuantity,
                        ReorderLevel = x.ReorderLevel
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


        public async Task<IResponseVm<string>> Update_Material(UpdateMaterial model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
                var IsVendorExist = _context.TblVendors.Where(x => x.VendorId == model.VendorId && x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
                var IsSKUExist = await AnyAsync(x => x.VendorId == model.VendorId && x.Sku == model.SKU && x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.RawMaterialId != Guid.Parse(model.MaterialId));
                if (IsSKUExist)
                {
                    responseVm.Message = "SKU" + " " +AppConstant.AlreadyExists;
                    return responseVm;
                }
               
                if (IsTenant)
                {
                    if (IsVendorExist != null)
                    {
                        var IsMaterialExists = GetAllQuerable()
                                 .Where(c => c.RawMaterialId == Guid.Parse(model.MaterialId)
                                               && c.TenantId == AppConstant.LoggedInTenantId
                                               && c.IsDelete == false)
                                   .FirstOrDefault();
                        if (IsMaterialExists != null)
                        {
                            IsMaterialExists.IsCutSheet = model.IsCutSheet;
                            IsMaterialExists.MaterialName = model.MaterialName;
                            IsMaterialExists.UnitCost = model.UnitCost;
                            IsMaterialExists.Sku = model.SKU;
                            IsMaterialExists.ProductType = Guid.Parse(model.ProductType);
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
                else
                {
                    responseVm.Message = AppConstant.AccessDenied;
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
