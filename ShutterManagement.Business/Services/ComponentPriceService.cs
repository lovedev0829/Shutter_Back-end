using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using MimeKit;
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
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class ComponentPriceService : BaseRepository<ShutterDBContext, MstComponentPrice>, IComponentPriceService
    {
        private readonly IOption optionService;
        private readonly IMaterial materialService;

        public ComponentPriceService(IOption optionService, IMaterial materialService)
        {
            this.optionService = optionService;
            this.materialService = materialService;
        }
        public async Task<IResponseVm<dynamic>> GetAllComponentsChip(string? Product, string? ProductType, string? Option)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                //var Component = (await GetAll()).Where(x => x.ProductType == Product && x.IsActive == true).ToList();
                List<ComponentPrice> ChipData = new List<ComponentPrice>();
                //var ChipData = Component.Select(x => new ComponentPrice
                //{
                //    Type = "Component",
                //    ComponentName = x.ComponentName,
                //    Name = "Component__" + x.ProductType + "__" + x.ComponentName,
                //}).ToList();
                Pagination filter = new Pagination();
                filter.ProductType = ProductType;
                filter.RecordPerPage = 100000;
                var Materials = await materialService.GetAll_Material(filter);
                if (Materials.IsSuccess)
                {
                    var MaterialData = Materials.Data.Select(x => new ComponentPrice
                    {
                        Type = "Materials",
                        ComponentName = x.MaterialName,
                        Name = "Material__" + x.ProductType + "__" + x.MaterialName
                    });
                    ChipData.AddRange(MaterialData);

                    var MaterialDataUnit = Materials.Data.Select(x => new ComponentPrice
                    {
                        Type = "Materials",
                        ComponentName = x.MaterialName + " - Unit",
                        Name = "Material__" + x.ProductType + "__" + x.MaterialName + "__Unit"
                    });
                    ChipData.AddRange(MaterialDataUnit);
                }
                if (!string.IsNullOrEmpty(Option))
                {
                    filter.SearchValue = Option;
                }
                //var Options = await optionService.GetAll_Option(filter);
                //if (Options.IsSuccess)
                //{
                //    var OptionData = Options.Data.Select(x => new ComponentPrice
                //    {
                //        Type = "Option",
                //        ComponentName = "Option - " +x.OptionName,
                //        Name = "Options__" + x.ProductTypeId + "__"+x.OptionName
                //    });
                //    ChipData.AddRange(OptionData);
                //}


                if (ChipData.Count() > 0)
                {
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.Data = ChipData.OrderBy(x => x.ComponentName);
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
        public async Task<IResponseVm<List<WindowProdcutDetailsForEstimate>>> GetProductDataForView(Guid WindowProductId, string EstimateID)
        {
            IResponseVm<List<WindowProdcutDetailsForEstimate>> responseVm = new ResponseVm<List<WindowProdcutDetailsForEstimate>>();

            try
            {
                var query =
                 (from estimate in dbContext.TblEstimates
                  join estimatewindow in dbContext.TblEstimateWindows on estimate.EstimateId equals estimatewindow.EstimateId
                  where estimatewindow.EstimateId.ToString() == EstimateID
                  join estwindowProduct in dbContext.TblEstimateWindowProducts on estimatewindow.WindowId equals estwindowProduct.WindowId
                  where estwindowProduct.EstimateId.ToString() == EstimateID
                  join estwinprocomp in dbContext.TblEstimateWindowProductComponents on estwindowProduct.ProductId equals estwinprocomp.ProductId into estwinprocomps
                  from estwinprocomp in estwinprocomps.DefaultIfEmpty()
                  where (string.IsNullOrEmpty(estwinprocomp.EstimateId.ToString()) || estwinprocomp.EstimateId.ToString() == EstimateID) &&
                  (string.IsNullOrEmpty(estwinprocomp.WindowId.ToString()) || estwindowProduct.WindowId == estwinprocomp.WindowId)
                  join productComponentmat in dbContext.TblEstimateWindowProductComponentMaterials on estwinprocomp.ComponentId equals
                  productComponentmat.ComponentId 
                  into productComponentmats from productComponentmat in productComponentmats.DefaultIfEmpty()
                  where (string.IsNullOrEmpty(productComponentmat.EstimateId.ToString()) || productComponentmat.EstimateId.ToString() == EstimateID ) &&
                  (string.IsNullOrEmpty(productComponentmat.WindowId.ToString()) || estwindowProduct.WindowId == productComponentmat.WindowId)
                  where estwindowProduct.WindowProductId == WindowProductId
                  && estimate.IsActive == true && estimate.IsDelete == false
                  && estimatewindow.IsActive == true && estimatewindow.IsDelete == false
                  && estwindowProduct.IsActive == true && estwindowProduct.IsDelete == false
                  //&& estwinprocomp.IsActive == true && estwinprocomp.IsDelete == false
                  //&& productComponentmat.IsActive == true && productComponentmat.IsDelete == false
                  select new WindowProdcutDetailsForEstimate
                  {
                      WindowId = (Guid)estimatewindow.WindowId,
                      AddressId = estimatewindow.AddressId,
                      Label = estimatewindow.Label,
                      Width = estimatewindow.Width,
                      Height = estimatewindow.Height,
                      ArchHeight = estimatewindow.ArchHeight,
                      Quantity = estimatewindow.Quantity,
                      JobId = estimate.JobId,
                      IsRated = estimatewindow.IsRated,
                      TotalPrice = estimate.TotalPrice,
                      // WindowProduct Properties
                      WindowProdcutId = estwindowProduct.WindowProductId,
                      ProductId = estwindowProduct.ProductId,
                      BuildWidth = estimatewindow.BuildWidth,
                      BuildHeight = estimatewindow.BuildHeight,

                      // Product Properties
                      ProductName = estwindowProduct.ProductName,
                      ProductWidth = estwindowProduct.Width,
                      ProductHeight = estwindowProduct.Height,
                      ProductArchHeight = estwindowProduct.ArchHeight,
                      Area = estwindowProduct.Area,
                      Length = estwindowProduct.Length,
                      AdditionalWidth = estwindowProduct.AdditionalWidth,
                      Markup = estwindowProduct.Markup,

                      // ProductType Properties
                      ProductTypeName = estwindowProduct.ProductTypeName,

                      // ProductComponent Properties
                      ComponentId = estwinprocomp.ComponentId,
                      //    ComponentQuantity = estwinprocomp.Quantity,

                      // Component Properties
                      ComponentName = estwinprocomp.ComponentName,
                      MarkupPercentage = estwinprocomp.MarkupPercentage,
                      PricingExpression = estwinprocomp.PricingExpression,
                      Price = estwinprocomp.ComponentPrice,

                      // Expression Properties
                      ExpressionFormulae = estwinprocomp.PricingExpression,

                      // ComponentMaterial Properties
                      RawMaterialId = productComponentmat.RawMaterialId,
                      // MaterialQuantity = componentMaterial.Quantity,
                      WasteFactor = estwinprocomp.Wastefactor,
                      AdditionalCost = estimate.AdditionalCost,
                      // Material Properties
                      MaterialName = productComponentmat.MaterialName,
                      UnitCost = productComponentmat.UnitCost,
                      // Non_Rated_UnitCost = material.NonRatedUnitCost,
                      UnitOfMeasurement = productComponentmat.UnitOfMeasurement,
                      StockQuantity = productComponentmat.StockQuantity,
                      ReorderLevel = productComponentmat.ReorderLevel,
                      IsCutSheet = productComponentmat.IsCutSheet
                  }).ToList();

                if (query != null && query.Count > 0)
                {
                    foreach (var item in query.DistinctBy(x => x.ProductId))
                    {
                        var Options = (from windowOption in dbContext.WindowOptions
                                       join optionType in dbContext.TblOptionsTypes on windowOption.OptionTypeId equals optionType.OptionTypeId
                                       join options in dbContext.TblOptions on optionType.OptionId equals options.OptionId
                                       where windowOption.WindowProductId == item.WindowProdcutId
                                       select new
                                       {
                                           OptionTypeId = optionType.OptionTypeId,
                                           OptionTypeName = Convert.ToString(options.OptionName + " - " + optionType.OptionType),
                                           OptionPrice = optionType.AdditionalCost ?? 0m
                                       }).ToList();
                        item.Options = Options.Select(x => x.OptionTypeName).ToArray();
                        item.OptionsPrice = Options.Select(x => x.OptionPrice).ToArray();
                    }
                }
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.FetchSuccessMsg;
                responseVm.Data = query;
                return responseVm;
            }
            //    else
            //    {
            //        responseVm.Message = AppConstant.NoDataFound;
            //    }
            //}
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<List<WindowProdcutDetailsForEstimate>>> GetProductData(Guid WindowProductId)
        {
            IResponseVm<List<WindowProdcutDetailsForEstimate>> responseVm = new ResponseVm<List<WindowProdcutDetailsForEstimate>>();

            try
            {
                var query =
                 (from window in dbContext.TblWindows
                  join windowProduct in dbContext.TblWindowProducts on window.WindowId equals windowProduct.WindowId
                  join product in dbContext.TblProducts on windowProduct.ProductId equals product.ProductId
                  join tax in dbContext.TblTaxRates on product.TaxId equals tax.TaxRateId into PTax
                  from taxdata in PTax.DefaultIfEmpty()
                //    from taxdata in PTax
                //.Where(t => t.TenantId == AppConstant.LoggedInTenantId && t.IsDelete==false)
                //.DefaultIfEmpty()

                  join productType in dbContext.MstProductTypes on product.ProductType equals productType.ProductId.ToString()
                  join productComponent in dbContext.TblProductComponents on product.ProductId equals productComponent.ProductId into productcomponents
                  from productComponent in productcomponents.DefaultIfEmpty()
                  .Where(x => x.IsActive == true)
                  join expression in dbContext.TblExpressions on productComponent.PricingExpression equals expression.Id.ToString() into PExpressions
                  from expression in PExpressions.DefaultIfEmpty()
                  join component in dbContext.TblComponents on productComponent.ComponentId equals component.ComponentId into components
                  from component in components.DefaultIfEmpty()
                  join componentMaterial in dbContext.TblComponentMaterials on component.ComponentId equals componentMaterial.ComponentId into MComponent
                  from componentMaterial in MComponent.DefaultIfEmpty()
                  join material in dbContext.TblRawMaterials on componentMaterial.RawMaterialId equals material.RawMaterialId into materials
                  from material in materials.DefaultIfEmpty()
                  where windowProduct.WindowProductId == WindowProductId
                  && window.IsActive == true && window.IsDelete == false
                  && windowProduct.IsActive == true && windowProduct.IsDelete == false
                  && product.IsActive == true && product.IsDelete == false
                  && productType.IsActive == true && productType.IsDelete == false

                  //&& productComponent.IsActive == true && productComponent.IsDelete == false
                  //&& component.IsActive == true && component.IsDelete == false
                  //&& expression.IsActive == true && expression.IsDelete == false
                  //&& componentMaterial.IsActive == true && componentMaterial.IsDelete == false
                  //&& material.IsActive == true && material.IsDelete == false
                  select new WindowProdcutDetailsForEstimate
                  {
                      // Window Properties
                      WindowId = window.WindowId,
                      AddressId = window.AddressId,
                      Label = window.Label,
                      Width = window.Width,
                      Height = window.Height,
                      ArchHeight = window.ArchHeight,
                      Quantity = window.Quantity,
                      JobId = window.JobId,
                      IsRated = window.IsRated,


                      // WindowProduct Properties
                      WindowProdcutId = windowProduct.WindowProductId,
                      ProductId = windowProduct.ProductId,
                      BuildWidth = windowProduct.BuildWidth,
                      BuildHeight = windowProduct.BuildHeight,

                      // Product Properties
                      ProductName = product.ProductName,
                      ProductWidth = product.Width,
                      ProductHeight = product.Height,
                      ProductArchHeight = product.ArchHeight,
                      Area = product.Area,
                      Length = product.Length,
                      AdditionalWidth = product.AdditionalWidth,
                      Markup = product.Markup,
                      IsComponentAvailable = product.IsComponentAvailable,

                      // ProductType Properties
                      ProductTypeName = productType.ProductName,

                      // ProductComponent Properties
                      ComponentId = productComponent.ComponentId,
                      ComponentQuantity = productComponent.Quantity,

                      // Component Properties
                      ComponentName = component.ComponentName,
                      MarkupPercentage = component.MarkupPercentage,
                      PricingExpression = productComponent.PricingExpression,
                      Price = component.Price,

                      // Expression Properties
                      ExpressionFormulae = expression.ExpressionFormulae,

                      // ComponentMaterial Properties
                      RawMaterialId = componentMaterial.RawMaterialId,
                      MaterialQuantity = componentMaterial.Quantity,
                      WasteFactor = componentMaterial.WasteFactor,

                      // Material Properties
                      MaterialName = material.MaterialName,
                      UnitCost = material.UnitCost,
                      Non_Rated_UnitCost = material.NonRatedUnitCost,
                      UnitOfMeasurement = material.UnitOfMeasurement,
                      StockQuantity = material.StockQuantity,
                      ReorderLevel = material.ReorderLevel,
                      IsCutSheet = material.IsCutSheet,

                      // Tax Details 
                      TaxRateId = taxdata.TaxRateId,
                      TaxName = taxdata.TaxName,
                      MunicipalityTax= taxdata.Municipality,
                      StateTax= taxdata.State,
                      CityTax = taxdata.City,
                      IsTaxAvailable = product.IsTaxApplied,



                  }).ToList();
                if (query != null && query.Count > 0)
                {
                    foreach (var item in query.DistinctBy(x => x.ProductId))
                    {

                        //for Partner
                        if (!string.IsNullOrEmpty(AppConstant.LoggedInParentTenantId))
                        {
                            var ManageProductDetails = dbContext.TblManagePartnersProducts
                                    .FirstOrDefault(x => x.ProductId == item.ProductId && x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);

                            if (ManageProductDetails != null)
                            {
                                var ProductTaxByPartner = dbContext.TblTaxRates.FirstOrDefault(x => x.TaxRateId == ManageProductDetails.TaxId);
                                if (ProductTaxByPartner != null)
                                {
                                    item.MunicipalityTax = ProductTaxByPartner.Municipality;
                                    item.StateTax = ProductTaxByPartner.State;
                                    item.CityTax = ProductTaxByPartner.City;
                                    item.TaxName = ProductTaxByPartner.TaxName;
                                    item.TaxRateId = ProductTaxByPartner.TaxRateId;
                                }
                                else
                                {
                                    item.MunicipalityTax = 0.0m;
                                    item.StateTax = 0.0m;
                                    item.CityTax = 0.0m;
                                    item.TaxName = "";
                                    item.TaxRateId = Guid.Empty;
                                }

                                var TaxOptionByPartner = from windowOptions in dbContext.WindowOptions
                                                         join partnerOptions in dbContext.TblManagePartnersProductsOptions on windowOptions.OptionTypeId equals partnerOptions.OptionTypeId
                                                         join optionValues in dbContext.TblOptionsTypes on partnerOptions.OptionTypeId equals optionValues.OptionTypeId
                                                         join options in dbContext.TblOptions on optionValues.OptionId equals options.OptionId
                                                         join taxDetails in dbContext.TblTaxRates on partnerOptions.TaxId equals taxDetails.TaxRateId into PTax
                                                         from tax in PTax.DefaultIfEmpty()
                                                         where partnerOptions.IsActive == true
                                                         && partnerOptions.TenantId == AppConstant.LoggedInTenantId &&
                                                         partnerOptions.ManageProductId == ManageProductDetails.ManageProductId &&
                                                         windowOptions.WindowId == item.WindowId &&
                                                         windowOptions.IsActive == true
                                                         select new OptionTaxVM
                                                         {
                                                             TaxId = (tax != null  && tax.TaxRateId != Guid.Empty ? tax.TaxRateId : item.TaxRateId) ?? item.TaxRateId ?? Guid.Empty,
                                                             TaxName = (tax.TaxRateId != Guid.Empty ? tax.TaxName : item.TaxName) ?? item.TaxName ?? "",
                                                             MunicipalityTax = (tax.TaxRateId != Guid.Empty ? tax.Municipality : item.MunicipalityTax) ?? item.MunicipalityTax ?? 0.0m,
                                                             StateTax = (tax.TaxRateId != Guid.Empty ? tax.State : item.StateTax) ?? item.StateTax ??  0.0m,
                                                             CityTax = (tax.TaxRateId != Guid.Empty ? tax.City : item.CityTax) ?? item.CityTax ?? 0.0m,
                                                             OptionPrice = optionValues.AdditionalCost,
                                                             OptionId = options.OptionId.ToString(),
                                                             OptionTypeId = optionValues.OptionTypeId.ToString(),
                                                             OptionName = options.OptionName ?? "",
                                                             OptionTypeName =  optionValues.OptionType ?? "",
                                                             WindowOptionId = windowOptions.WindowoptionId,
                                                             //IsTaxAvailable = opt.IsTaxApplied ?? false
                                                         };

                                item.OptionTaxDetails = TaxOptionByPartner != null ? TaxOptionByPartner.ToList() : null;

                            }

                        }

                        else
                        {
                            var TaxOptionForTenant = from windowOptions in dbContext.WindowOptions
                                                     join optionValues in dbContext.TblOptionsTypes on windowOptions.OptionTypeId equals optionValues.OptionTypeId
                                                     join options in dbContext.TblOptions on optionValues.OptionId equals options.OptionId
                                                    join taxDetails in dbContext.TblTaxRates on options.TaxId equals taxDetails.TaxRateId into PTax
                                                         from tax in PTax.DefaultIfEmpty()
                                                     where tax.TenantId == AppConstant.LoggedInTenantId && options.IsActive == true
                                                     && windowOptions.TenantId == AppConstant.LoggedInTenantId &&
                                                      windowOptions.WindowId == item.WindowId &&
                                                     options.ProductId == item.ProductId &&
                                                     windowOptions.IsActive == true
                                                     select new OptionTaxVM
                                                     {
                                                         TaxId = (tax.TaxRateId != Guid.Empty ? tax.TaxRateId : item.TaxRateId) ?? Guid.Empty,
                                                         TaxName = (tax.TaxRateId != Guid.Empty ? tax.TaxName : item.TaxName) ?? "",
                                                         MunicipalityTax = (tax.TaxRateId != Guid.Empty ? tax.Municipality : item.MunicipalityTax) ?? 0.0m,
                                                         StateTax = (tax.TaxRateId != Guid.Empty ? tax.State : item.StateTax) ?? 0.0m,
                                                         CityTax = (tax.TaxRateId != Guid.Empty ? tax.City : item.CityTax) ?? 0.0m,
                                                         OptionPrice = optionValues.AdditionalCost,
                                                         OptionId = options.OptionId.ToString(),
                                                         OptionTypeId = optionValues.OptionTypeId.ToString(),
                                                         OptionName = options.OptionName ?? "",
                                                         OptionTypeName =  optionValues.OptionType ?? "",
                                                         WindowOptionId = windowOptions.WindowoptionId,
                                                        // IsTaxAvailable = options.IsTaxApplied ?? false
                                                     };
                            item.OptionTaxDetails = TaxOptionForTenant.ToList();
                          
                        }
                        item.OptionsTypeIds = item.OptionTaxDetails.Select(x => x.OptionTypeId.ToString()).ToArray();
                        item.OptionIds = item.OptionTaxDetails.Select(x => x.OptionId.ToString()).ToArray();
                        item.Options = item.OptionTaxDetails.Select(x => x.OptionName + " - " + x.OptionTypeName).ToArray();
                    }
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.Data = query;
                    return responseVm;
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




        private OptionTaxVM CalculateOptionTax(dynamic option, WindowProdcutDetailsForEstimate item)
        {
            var taxId = (option.IsTaxApplied == true && option.TaxRateId != null) ? option.TaxRateId : item.TaxRateId;
            var cityTaxRate = (option.IsTaxApplied == true && option.CityTax > 0) ? option.CityTax : item.CityTax;
            var stateTaxRate = (option.IsTaxApplied == true && option.StateTax > 0) ? option.StateTax : item.StateTax;
            var municipalityTaxRate = (option.IsTaxApplied == true && option.Municipality > 0) ? option.Municipality : item.MunicipalityTax;

            var cityTaxAmount = (option.OptionPrice * cityTaxRate) / 100;
            var stateTaxAmount = (option.OptionPrice * stateTaxRate) / 100;
            var municipalityTaxAmount = (option.OptionPrice * municipalityTaxRate) / 100;

            return new OptionTaxVM
            {
                OptionId = option.Optionid.ToString(),
                WindowOptionId = option.WindowOptionId,
                TaxId = taxId,
                TaxName = (option.IsTaxApplied == true && !string.IsNullOrEmpty(option.TaxName)) ? option.TaxName : item.TaxName,
                OptionPrice = option.OptionPrice,
                CityTaxAmount = cityTaxAmount??0.0m,
                StateTaxAmount = stateTaxAmount ?? 0.0m,
                MunicipalityTaxAmount = municipalityTaxAmount ?? 0.0m,
                IsTaxAvailable = option.IsTaxApplied
            };
        }

    }
}
