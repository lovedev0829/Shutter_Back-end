using Azure;
using Microsoft.Extensions.Configuration;
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
using System.Data.Entity;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace ShutterManagement.Business.Services
{
    public class MaterialService : BaseRepository<ShutterDBContext, TblRawMaterial>, IMaterial
    {
        private readonly ShutterDBContext _context;
        private readonly IVendorService vendorService;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;
        private readonly IComponentMaterial componentMaterial;
        private readonly IUnitofMeasurementService unitofMeasurementService;

        public MaterialService(ShutterDBContext context, IVendorService vendorService,
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (IsTenant)
                {
                    var IsMaterialExists = GetAllQuerable()
                          .Where(c => c.RawMaterialId == Id
                                      && c.TenantId == claimsProperty.GetTenantId()
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
               
                if(IsTenant)
                {
                    var IsSkuExist = false;
                    var VendorExit = vendorService.GetAllQuerable().Where(v => (v.VendorCode == model.Vendor || v.CompanyName == model.Vendor) && v.IsDelete == false && v.TenantId == claimsProperty.GetTenantId())
                                   .FirstOrDefault();
                    var IsMaterialNameExist = Any(x => x.MaterialName.ToLower().Trim() == model.MaterialName.ToLower().Trim() && x.MaterialUniqueId != model.MaterialUniqueId && x.TenantId == claimsProperty.GetTenantId() && x.IsDelete == false);
                    var GetAllVendorList = vendorService.GetAllQuerable()
                    .Where(x => x.IsActive == true && x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId()).ToList();
                    var GetAllUnitOfMeasureList = unitofMeasurementService.GetAllQuerable()
                    .Where(x => x.IsActive == true && x.IsDelete == false).ToList();
                    var CheckExistingVendorbyCode = GetAllVendorList.Any(x => x.VendorCode == model.Vendor || x.CompanyName==model.Vendor);
                    if(!string.IsNullOrEmpty(model.SKU))
                    {
                       IsSkuExist = Any(x => x.Sku.ToLower().Trim() == model.SKU.ToLower().Trim() && x.MaterialUniqueId != model.MaterialUniqueId && x.TenantId == claimsProperty.GetTenantId() && x.IsDelete == false);
                    }
                    if(IsSkuExist)
                    {
                        responseVm.Message = $"SKU {model.SKU} " + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    if (IsMaterialNameExist)
                    {
                        responseVm.Message = $"Material Name {model.MaterialName} " + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    if(VendorExit==null)
                    {
                        responseVm.Message = $"Vendor {model.Vendor} Not Valid. " ;
                        return responseVm;
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
        public async Task<IResponseVm<string>> Create_Material(CreateMaterial model,Guid? LoggedInTenandId = null)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var tenantId = GetValidTenantId(LoggedInTenandId);
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == tenantId && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
                var IsVendorExist = await vendorService.AnyAsync(x => x.VendorId == model.VendorId && x.TenantId == tenantId && x.IsActive == true && x.IsDelete == false);
                
              
                if (IsTenant)
                {
                    if (model.TotalCost < 0)
                    {
                        responseVm.Message = "Total Cost should be greater than 0";
                        return responseVm;
                    }
                    if (model.MaterialUniqueId != null)
                    {
                        var IsMaterialUniqueId = GetAllQuerable().Where(x => x.MaterialUniqueId.ToLower() == model.MaterialUniqueId.ToLower() && x.IsActive == true
                        && x.IsDelete == false).FirstOrDefault();
                        if (IsMaterialUniqueId != null)
                        {
                            decimal? length = null;
                            decimal? width = null;
                            
                            (length, width) = UnitOfMeasurementHelper.CalculateDimensions(model.UnitOfMeasurement, model.Lenght, model.Width);

                            var res = new UpdateMaterial
                            {
                                MaterialName = model.MaterialName,
                                UnitCost = model.UnitCost,
                                Lenght = length,
                                Width = width,
                                SKU = model.SKU,
                                ProductType = model.ProductType,
                                UnitOfMeasurement = model.UnitOfMeasurement,
                                StockQuantity = model.StockQuantity,
                                ReorderLevel = model.ReorderLevel,
                                VendorId = model.VendorId,
                                Notes = model.Notes,
                                TotalCost = model.TotalCost,
                                IsCutSheet = model.IsCutSheet,
                                MaterialUniqueId = model.MaterialUniqueId
                            };
                            var res1 = await Update_Material(res,tenantId);
                            if(res1 != null)
                            {
                                responseVm.IsSuccess = true;
                                responseVm.Message = res1.Message;
                                return responseVm;
                            }
                        }
                        else
                        {
                            if (IsVendorExist)
                            {
                                int Version1 = 0;
                                var MaterialUniqueId1 = GetAllQuerable().Where(x => x.TenantId == tenantId).OrderByDescending(p => p.CreatedAt).
                                    Select(x => x.MaterialUniqueId).FirstOrDefault();
                                if (MaterialUniqueId1 != null)
                                {
                                    Version1 = MaterialUniqueId1.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(MaterialUniqueId1.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0;
                                }
                                Version1 += 1;
                                var generateUniqueId = GenerateUniqueId.GenerateUniqueIds(ModuleName.Material, tenantId.ToString(), Version1);
                                decimal? unitPrice = UnitOfMeasurementHelper.CalculateUnitPrice(model.UnitOfMeasurement, model.Lenght, model.Width, model.TotalCost);
                                TblRawMaterial material = new TblRawMaterial
                                {
                                    MaterialName = model.MaterialName.Trim(),
                                    UnitCost = unitPrice??0.0m,
                                    Length = model.Lenght ?? null,
                                    Width = model.Width ?? null,
                                    NonRatedUnitCost =unitPrice??0.0m,
                                    UnitOfMeasurement = model.UnitOfMeasurement,
                                    StockQuantity = model.StockQuantity,
                                    ReorderLevel = model.ReorderLevel,
                                    ProductType = Guid.Parse(model.ProductType),
                                    Sku = model.SKU ?? "",
                                    IsCutSheet = model.IsCutSheet,
                                    VendorId = model.VendorId,
                                    Notes = model.Notes,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = AppConstant.LoggedInUserID,
                                    IsActive = true,
                                    IsDelete = false,
                                    TenantId = tenantId,
                                    MaterialUniqueId = generateUniqueId,
                                    TotalCost=model.TotalCost ?? 0.0m,
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
                                    return responseVm;
                                }
                            }

                        }
                    }
                    var IsSKUExist = Any(x => x.VendorId == model.VendorId && x.Sku == model.SKU && x.TenantId == tenantId && x.IsDelete == false && model.SKU != null);
                    var IsMaterialExist = Any(x => x.MaterialName.ToLower().Trim() == model.MaterialName.ToLower().Trim() && x.TenantId == tenantId && x.IsDelete == false);
                    if (IsMaterialExist)
                    {
                        responseVm.Message = $"Material Name" + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    if (IsSKUExist)
                    {
                        responseVm.Message = $"SKU {model.SKU}" + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    int Version = 0;
                    var MaterialUniqueId = GetAllQuerable().Where(x => x.TenantId == tenantId).OrderByDescending(p => p.CreatedAt).
                        Select(x => x.MaterialUniqueId).FirstOrDefault();
                    if (MaterialUniqueId != null)
                    {
                        Version = MaterialUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries).Length == 3 ? Convert.ToInt32(MaterialUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries)[2]) : 0;
                    }
                    Version += 1;
                    var generateUnique = GenerateUniqueId.GenerateUniqueIds(ModuleName.Material, claimsProperty.GetCompanyCode(), Version);
                    if (IsVendorExist)
                    {
                       

                        decimal? unitPrice = UnitOfMeasurementHelper.CalculateUnitPrice(model.UnitOfMeasurement,model.Lenght,model.Width,model.TotalCost);

                        TblRawMaterial material = new TblRawMaterial
                        {
                            MaterialName = model.MaterialName.Trim(),
                            UnitCost = unitPrice??0.0m,
                            Length=model.Lenght?? null,
                            Width=model.Width?? null,
                            NonRatedUnitCost = unitPrice ??0.0m,
                            UnitOfMeasurement = model.UnitOfMeasurement,
                            StockQuantity = model.StockQuantity,
                            ReorderLevel = model.ReorderLevel,
                            ProductType = Guid.Parse(model.ProductType),
                            Sku = model.SKU?? "",
                            IsCutSheet = model.IsCutSheet,
                            VendorId = model.VendorId,
                            Notes = model.Notes,
                            CreatedAt = DateTime.Now,
                            CreatedBy = AppConstant.LoggedInUserID,
                            IsActive = true,
                            IsDelete = false,
                            TenantId = tenantId,
                            MaterialUniqueId = generateUnique,
                            TotalCost = model.TotalCost ?? 0.0m,
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.ParentTenantId == null && x.IsDelete == false && x.IsActive == true);
                if (IsTenant)
                {
                    var IsMaterialExists =  GetAllQuerable()
                          .Where(c => c.RawMaterialId == Id
                                      && c.TenantId == claimsProperty.GetTenantId()
                                      && c.IsDelete == false 
                                      )
                          .FirstOrDefault();

                    bool isMaterialUsed =
                   dbContext.TblComponentMaterials.Any(p => p.RawMaterialId == Id && p.IsDelete == false);
                    if (isMaterialUsed)
                    {
                        responseVm.Message = "This Material is used in another entity (Component) and cannot be deleted.";
                        responseVm.IsSuccess = false;
                        return responseVm;
                    }

                    var IsMaterialExistInComponent = componentMaterial.AnyAsync(x => x.RawMaterialId == Id && x.IsActive == true && x.IsDelete == false && x.TenantId == claimsProperty.GetTenantId());
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
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsDelete == false && x.IsActive == true);

                if (IsTenant)
                {

                    var MaterialList = (from t2 in _context.TblRawMaterials.OrderByDescending(x => x.CreatedAt)
                                        join t1 in _context.TblVendors on t2.VendorId equals t1.VendorId
                                        where t2.TenantId == claimsProperty.GetTenantId() && t2.IsDelete == false
                                        select new GetAllMaterialVm
                                        {
                                            IsActive = t2.IsActive,
                                            MaterialId = t2.RawMaterialId,
                                            MaterialName = t2.MaterialName,
                                            Notes = t2.Notes,
                                            ReorderLevel = t2.ReorderLevel,
                                            SKU = t2.Sku,
                                            Lenght=t2.Length??null,
                                            Width=t2.Width ?? null,
                                            StockQuantity = t2.StockQuantity,
                                            UnitCost = t2.UnitCost,
                                            //NonRatedUnitCost = t2.NonRatedUnitCost,
                                            ProductType = t2.ProductType,
                                            UnitOfMeasurement = t2.UnitOfMeasurement,
                                            VendorId = t2.VendorId,
                                            FirstName = t1.FirstName,
                                            LastName = t1.LastName,
                                            CompanyName=t1.CompanyName,
                                            IsCutSheet = t2.IsCutSheet,
                                            MaterialUniqueId = t2.MaterialUniqueId,
                                            TotalCost = t2.TotalCost??0.0m,
                                        }).ToList();

                    FileLogger.Log($"This is Material List {MaterialList}");
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
                                        //|| (x.NonRatedUnitCost.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.ReorderLevel.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.StockQuantity.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.Notes.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.CompanyName.ToString() ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.SKU ?? "").ToLower().Contains(filter.SearchValue)
                                        || ((x.FirstName.ToLower() + " " + x.LastName.ToLower()) ?? "").Contains(filter.SearchValue) 
                                        || (x.FirstName.ToLower() ?? "").Contains(filter.SearchValue)
                                        || (x.LastName.ToLower() ?? "").Contains(filter.SearchValue) 
                                        || (x.UnitOfMeasurement ?? "").ToLower().Contains(filter.SearchValue)))
                                    .ToList();

                    

                    responseVm.Data = FilterRecords.Select(x => new GetAllMaterialVm
                    {
                        MaterialName = x.MaterialName??"",
                        IsActive = x.IsActive,
                        VendorId = x.VendorId,
                        TenantId=x.TenantId,
                        ProductType = x.ProductType,
                        Lenght=x.Lenght??null,
                        Width=x.Width??null,
                        Notes = x.Notes ?? "",
                        UnitCost= x.UnitCost,
                        NonRatedUnitCost = x.NonRatedUnitCost,
                        UnitOfMeasurement = x.UnitOfMeasurement,
                        StockQuantity = x.StockQuantity,
                        ReorderLevel = x.ReorderLevel,
                        SKU=x.SKU?? "", 
                        MaterialUniqueId = x.MaterialUniqueId ?? "",
                    }).ToList();

                    var queryable = FilterRecords.AsQueryable();

                    queryable = Paginate.ApplyColumnFilters(queryable, filter.ColumnFilters);
                    queryable = Paginate.ApplySorting(queryable, filter.SortColumn, filter.SortDirection);

                    var TakenRecords = Paginate.Pagination(queryable.ToList(), filter);

                    FileLogger.Log($"This is Material List  ResponseVm data {responseVm.Data}");
                    responseVm.TotalRecords = FilterRecords.Count;
                    responseVm.Data = TakenRecords;
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
                FileLogger.Log($"Error Material fetch: {AppConstant.ExceptionError + ex.Message}");
            }

            return responseVm;
        }
        public async Task<IResponseVm<string>> Update_Material(UpdateMaterial model,Guid? LoggedTenantId = null)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var tenantId = GetValidTenantId(LoggedTenantId);
                Guid MaterialId = Guid.Empty;
                Guid.TryParse(model.MaterialId, out MaterialId);
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == tenantId && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
                var IsVendorExist = vendorService.GetAllQuerable().Where(x => x.VendorId == model.VendorId && x.TenantId == tenantId && x.IsActive == true && x.IsDelete == false);
                var IsSKUExist = !string.IsNullOrEmpty(model.SKU) ? (await AnyAsync(x => x.VendorId == model.VendorId && x.Sku == model.SKU && x.TenantId == tenantId
                && x.IsDelete == false && (!string.IsNullOrEmpty(model.MaterialId) ||
                MaterialId != Guid.Empty ? x.RawMaterialId != MaterialId : x.MaterialUniqueId != model.MaterialUniqueId))) : false;
                var IsMaterialExist = Any(x => x.MaterialName.ToLower().Trim() == model.MaterialName.ToLower().Trim() &&
                x.TenantId == tenantId &&
                x.IsDelete == false && (!string.IsNullOrEmpty(model.MaterialId) ||
                MaterialId != Guid.Empty ? x.RawMaterialId != MaterialId : x.MaterialUniqueId != model.MaterialUniqueId));
                if (IsMaterialExist)
                {
                    responseVm.Message = $"Material Name" + AppConstant.AlreadyExists;
                    return responseVm;
                }
                if (IsSKUExist)
                {
                    responseVm.Message = "SKU" + " " + AppConstant.AlreadyExists;
                    return responseVm;
                }
                if (model.TotalCost < 0)
                {
                    responseVm.Message = "Total Cost should be greater than 0";
                    return responseVm;
                }
                if (IsTenant)
                {
                    if (IsVendorExist != null)
                    {

                        var IsMaterialExists = GetAllQuerable()
                                 .Where(c => (!string.IsNullOrEmpty(model.MaterialId) ? c.RawMaterialId == Guid.Parse(model.MaterialId) : c.MaterialUniqueId == model.MaterialUniqueId)
                                               && c.TenantId == tenantId
                                               && c.IsDelete == false)
                                   .FirstOrDefault();
                        decimal? length = null;
                        decimal? width = null;

                                                    (length, width) = UnitOfMeasurementHelper.CalculateDimensions(
                                model.UnitOfMeasurement, 
                                model.Lenght, 
                                model.Width);
                        decimal? unitPrice = UnitOfMeasurementHelper.CalculateUnitPrice(model.UnitOfMeasurement, model.Lenght, model.Width, model.TotalCost);

                        if (IsMaterialExists != null)
                        {
                            IsMaterialExists.IsCutSheet = model.IsCutSheet;
                            IsMaterialExists.MaterialName = model.MaterialName;
                            IsMaterialExists.UnitCost = unitPrice??0.0m;
                            IsMaterialExists.Length = length;
                            IsMaterialExists.Width = width;
                            // IsMaterialExists.NonRatedUnitCost = model.NonRatedUnitCost;286
                            IsMaterialExists.Sku = model.SKU;
                            IsMaterialExists.ProductType = Guid.Parse(model.ProductType);
                            IsMaterialExists.UnitOfMeasurement = model.UnitOfMeasurement;
                            IsMaterialExists.StockQuantity = model.StockQuantity;
                            IsMaterialExists.ReorderLevel = model.ReorderLevel;
                            IsMaterialExists.VendorId = model.VendorId;
                            IsMaterialExists.Notes = model.Notes;
                            IsMaterialExists.ModifiedBy = AppConstant.LoggedInUserID;
                            IsMaterialExists.UpdatedAt = DateTime.Now;
                            IsMaterialExists.TotalCost = model.TotalCost??0.0m;
                            

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

        private Guid? GetValidTenantId(Guid? tenantId)
        {
            return (tenantId != Guid.Empty && tenantId != null) ? tenantId : claimsProperty.GetTenantId();
        }
        public int GetNextMaterialVersion()
        {
            int version = 0;

            var materialUniqueId = GetAllQuerable()
                .Where(x => x.TenantId == claimsProperty.GetTenantId()
                            && x.CreatedAt.HasValue
                            && x.CreatedAt.Value.Date == DateTime.Today)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(materialUniqueId.MaterialUniqueId))
            {
                var parts = materialUniqueId.MaterialUniqueId.Split("-", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3 && int.TryParse(parts[2], out int parsed))
                {
                    version = parsed;
                }
            }

            
            return version + 1;


        }


    }
}
