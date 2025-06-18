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
using System.Data.Entity;
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
        private readonly ClaimsProperty claimsProperty;

        public ComponentPriceService(IOption optionService, IMaterial materialService,ClaimsProperty claimsProperty)
        {
            this.optionService = optionService;
            this.materialService = materialService;
            this.claimsProperty = claimsProperty;
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
                        Name = "Material__" + x.ProductType + "__" + x.MaterialId
                    });
                    ChipData.AddRange(MaterialData);

                    var MaterialDataUnit = Materials.Data.Select(x => new ComponentPrice
                    {
                        Type = "Materials",
                        ComponentName = x.MaterialName + " - Unit",
                        Name = "Material__" + x.ProductType + "__" + x.MaterialId + "__Unit"
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
                      TotalTaxAmount = estimate.TotaltaxAmount,
                      // WindowProduct Properties
                      
                      WindowProdcutId = estwindowProduct.WindowProductId,
                      ProductId = estwindowProduct.ProductId,
                      BuildWidth = estimatewindow.BuildWidth,
                      BuildHeight = estimatewindow.BuildHeight,
                      EstimateWindowProductId = estwindowProduct.EstimateWindowProductId,
                      ComponentMarkup = estwindowProduct.ComponentMarkup,
                      ProductMarkup = estwindowProduct.ProductMarkup,
                      OptionMarkup = estwindowProduct.OptionMarkup,
                      // Product Properties
                      ProductName = estwindowProduct.ProductName,
                      ProductWidth = estwindowProduct.Width,
                      ProductHeight = estwindowProduct.Height,
                      ProductArchHeight = estwindowProduct.ArchHeight,
                      Area = estwindowProduct.Area,
                      Length = estwindowProduct.Length,
                      AdditionalWidth = estwindowProduct.AdditionalWidth,
                      Markup = estwindowProduct.Markup,
                      ProductPrice = estwindowProduct.ProductPrice,

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
                    var distinctProducts = query.DistinctBy(x => x.ProductId).ToList();

                    foreach (var distinctItem in distinctProducts)
                    {
                        var Options = (from estimateOption in dbContext.TblEstimateWindowProductOptions
                                       join optionType in dbContext.TblOptionsTypes on estimateOption.OptionTypeid equals optionType.OptionTypeId
                                       join option in dbContext.TblOptions on optionType.OptionId equals option.OptionId
                                       where estimateOption.EstimateWindowProductId == distinctItem.EstimateWindowProductId
                                       select new OptionTaxVM
                                       {
                                           TaxId = estimateOption.TaxId,
                                           TaxName = estimateOption.TaxName ?? "",
                                           MunicipalityTax = estimateOption.MunicipilityTax,
                                           StateTax = estimateOption.StateTax,
                                           CityTax = estimateOption.CityTax,
                                           OptionPrice = estimateOption.OptionPrice,
                                           OptionName = estimateOption.OptionName ?? "",
                                           OptionId = estimateOption.Optionid.ToString(),
                                           OptionTypeId = estimateOption.OptionTypeid.ToString(),
                                           OptionTypeName = estimateOption.OptionTypeName,
                                       }).ToList();
                        distinctItem.OptionTaxDetails = Options;
                        // Propagate to ALL items with the same ProductId
                        var matchingItems = query.Where(x => x.ProductId == distinctItem.ProductId).ToList();
                        foreach (var item in matchingItems)
                        {
                            item.Options = distinctItem.OptionTaxDetails.Select(x => x.OptionName + " - " + x.OptionTypeName).ToArray();
                            item.AdditionalCost = Options.Select(x => x.OptionPrice).Sum();
                            //item.OptionTaxDetails = Options;
                        }
                    }

                    //foreach (var distinctItem in query)
                    ////foreach (var distinctItem in query.DistinctBy(x =>x.ProductId))
                    //{
                    //    var Options = (from estimateOption in dbContext.TblEstimateWindowProductOptions
                    //                   join optionType in dbContext.TblOptionsTypes on estimateOption.OptionTypeid equals optionType.OptionTypeId
                    //                   join option in dbContext.TblOptions on optionType.OptionId equals option.OptionId
                    //                   where estimateOption.EstimateWindowProductId == distinctItem.EstimateWindowProductId
                    //                   select new OptionTaxVM
                    //                   {
                    //                       TaxId = estimateOption.TaxId,
                    //                       TaxName = estimateOption.TaxName ?? "",
                    //                       MunicipalityTax = estimateOption.MunicipilityTax,
                    //                       StateTax = estimateOption.StateTax,
                    //                       CityTax = estimateOption.CityTax,
                    //                       OptionPrice = estimateOption.OptionPrice,
                    //                       OptionName = estimateOption.OptionName ?? "",
                    //                       OptionId = estimateOption.Optionid.ToString(),
                    //                       OptionTypeId = estimateOption.OptionTypeid.ToString(),
                    //                       OptionTypeName = estimateOption.OptionTypeName,
                    //                   }).ToList();
                    //    distinctItem.Options = Options.Select(x => x.OptionTypeName).ToArray();
                    //    distinctItem.AdditionalCost = Options.Select(x => x.OptionPrice).Sum();
                    //    distinctItem.OptionTaxDetails = Options;
                    //    //distinctItem.OptionsPrice = Options.Select(x => x.OptionPrice).ToArray();
                    //}
                }
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.FetchSuccessMsg;
                responseVm.Data = query.DistinctBy(y => y.ComponentId).ToList();
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
        public async Task<IResponseVm<List<WindowProdcutDetailsForEstimate>>> GetProductData(Guid windowProductId)
        {
            var responseVm = new ResponseVm<List<WindowProdcutDetailsForEstimate>>();

            try
            {
                var query = (from window in dbContext.TblWindows
                             join windowProduct in dbContext.TblWindowProducts on window.WindowId equals windowProduct.WindowId
                             join product in dbContext.TblProducts on windowProduct.ProductId equals product.ProductId
                             join tax in dbContext.TblTaxRates.Where(x => x.IsActive == true) on product.TaxId equals tax.TaxRateId into taxJoin
                             from taxdata in taxJoin.DefaultIfEmpty()
                             join productType in dbContext.MstProductTypes on product.ProductType equals productType.ProductId.ToString()
                             join productComponent in dbContext.TblProductComponents.Where(x => x.IsActive == true) on product.ProductId equals productComponent.ProductId into componentJoin
                             from productComponent in componentJoin.DefaultIfEmpty()
                             join componentExpression in dbContext.TblExpressions.Where(y => y.IsActive == true) on productComponent.PricingExpression equals componentExpression.Id.ToString() into expressionJoin
                             from componentExpression in expressionJoin.DefaultIfEmpty()
                             join component in dbContext.TblComponents.Where(y => y.IsActive == true) on productComponent.ComponentId equals component.ComponentId into compJoin
                             from component in compJoin.DefaultIfEmpty()
                             join componentMaterial in dbContext.TblComponentMaterials.Where(y => y.IsActive == true) on component.ComponentId equals componentMaterial.ComponentId into compMatJoin
                             from componentMaterial in compMatJoin.DefaultIfEmpty()
                             join materialExpression in dbContext.TblExpressions.Where(y => y.IsActive == true) on componentMaterial.MaterialPricingExpression equals materialExpression.Id into materialExpressionJoin
                             from materialExpression in materialExpressionJoin.DefaultIfEmpty()
                             join material in dbContext.TblRawMaterials.Where(y => y.IsActive == true) on componentMaterial.RawMaterialId equals material.RawMaterialId into materialJoin
                             from material in materialJoin.DefaultIfEmpty()
                             where windowProduct.WindowProductId == windowProductId
                             && window.IsActive == true && window.IsDelete == false
                             && windowProduct.IsActive == true && windowProduct.IsDelete == false
                             && product.IsActive == true && product.IsDelete == false
                             && productType.IsActive == true && productType.IsDelete == false
                             select new WindowProdcutDetailsForEstimate
                             {
                                 WindowId = window.WindowId,
                                 AddressId = window.AddressId,
                                 Label = window.Label,
                                 Width = window.Width,
                                 Height = window.Height,
                                 ArchHeight = window.ArchHeight,
                                 Quantity = window.Quantity,
                                 JobId = window.JobId,
                                 IsRated = window.IsRated,
                                 WindowProdcutId = windowProduct.WindowProductId,
                                 ProductId = windowProduct.ProductId,
                                 BuildWidth = windowProduct.BuildWidth,
                                 BuildHeight = windowProduct.BuildHeight,
                                 ProductName = product.ProductName,
                                 ProductWidth = product.Width,
                                 ProductHeight = product.Height,
                                 ProductArchHeight = product.ArchHeight,
                                 Area = product.Area,
                                 Length = product.Length,
                                 AdditionalWidth = product.AdditionalWidth,
                                 Markup = product.Markup,
                                 IsComponentAvailable = product.IsComponentAvailable,
                                 ProductTypeName = productType.ProductName,
                                 ComponentId = component.ComponentId,
                                 ComponentQuantity = productComponent.Quantity,
                                 ComponentName = component.ComponentName,
                                 MarkupPercentage = component.MarkupPercentage,
                                 PricingExpression = productComponent.PricingExpression,
                                 MaterialPricingExpression = componentMaterial.MaterialPricingExpression.ToString(),
                                 Price = component.Price,
                                 ExpressionFormulae = componentExpression.ExpressionFormulae,
                                 MaterialExpressionFormulae = materialExpression.ExpressionFormulae,
                                 RawMaterialId = componentMaterial.RawMaterialId,
                                 MaterialQuantity = componentMaterial.Quantity,
                                 WasteFactor = componentMaterial.WasteFactor,
                                 MaterialName = material.MaterialName,
                                 UnitCost = material.UnitCost,
                                 Non_Rated_UnitCost = material.NonRatedUnitCost,
                                 UnitOfMeasurement = material.UnitOfMeasurement,
                                 StockQuantity = material.StockQuantity,
                                 ReorderLevel = material.ReorderLevel,
                                 IsCutSheet = material.IsCutSheet,
                                 TaxRateId = taxdata.TaxRateId,
                                 TaxName = taxdata.TaxName,
                                 MunicipalityTax = taxdata.Municipality,
                                 StateTax = taxdata.State,
                                 CityTax = taxdata.City,
                                 IsTaxAvailable = product.IsTaxApplied
                             }).ToList();

                if (!query.Any())
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }

                var distinctProducts = query.DistinctBy(x => x.ProductId).ToList();

                foreach (var product in distinctProducts)
                {
                    var optionTaxDetails = await GetOptionTaxDetails(product);
                    product.OptionTaxDetails = optionTaxDetails.ToList();

                    var relatedItems = query.Where(x => x.ProductId == product.ProductId).ToList();
                    foreach (var item in relatedItems)
                    {
                        item.AdditionalCost = optionTaxDetails.Sum(x => x.OptionPrice);
                        item.OptionsTypeIds = optionTaxDetails.Select(x => x.OptionTypeId).ToArray();
                        item.OptionIds = optionTaxDetails.Select(x => x.OptionId).ToArray();
                        item.Options = optionTaxDetails.Select(x => x.OptionName + " - " + x.OptionTypeName).ToArray();
                    }
                }

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.FetchSuccessMsg;
                responseVm.Data = query;
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        private async Task<List<OptionTaxVM>> GetOptionTaxDetails(WindowProdcutDetailsForEstimate product)
        {
            List<OptionTaxVM> optionTaxDetails;

            if (!string.IsNullOrEmpty(claimsProperty.GetParentTenantId()))
            {
                var manageProduct = dbContext.TblManagePartnersProducts
                    .FirstOrDefault(x => x.ProductId == product.ProductId && x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

                if (manageProduct == null)
                    return new List<OptionTaxVM>();

                optionTaxDetails = (from windowOptions in dbContext.WindowOptions
                                    join partnerOptions in dbContext.TblManagePartnersProductsOptions on windowOptions.OptionTypeId equals partnerOptions.OptionTypeId
                                    join optionValues in dbContext.TblOptionsTypes on partnerOptions.OptionTypeId equals optionValues.OptionTypeId
                                    join options in dbContext.TblOptions on optionValues.OptionId equals options.OptionId
                                    join taxDetails in dbContext.TblTaxRates.Where(x => x.TenantId == claimsProperty.GetTenantId()) on
                                    (partnerOptions.TaxId == null || partnerOptions.TaxId == Guid.Empty ? manageProduct.TaxId : partnerOptions.TaxId)
                                    equals taxDetails.TaxRateId into taxJoin
                                    from tax in taxJoin.DefaultIfEmpty()
                                    where partnerOptions.IsActive == true && partnerOptions.TenantId == claimsProperty.GetTenantId()
                                          && partnerOptions.ManageProductId == manageProduct.ManageProductId
                                          && windowOptions.WindowId == product.WindowId
                                          && windowOptions.IsActive == true
                                    select new OptionTaxVM
                                    {
                                        TaxId = (tax != null && tax.TaxRateId != Guid.Empty ? tax.TaxRateId : manageProduct.TaxId) ?? Guid.Empty,
                                        TaxName = (tax.TaxRateId != Guid.Empty ? tax.TaxName : tax.TaxName) ?? "",
                                        MunicipalityTax = (tax.TaxRateId != Guid.Empty ? tax.Municipality : tax.Municipality) ?? 0.0m,
                                        StateTax = (tax.TaxRateId != Guid.Empty ? tax.State : tax.State) ?? 0.0m,
                                        CityTax = (tax.TaxRateId != Guid.Empty ? tax.City : tax.City) ?? 0.0m,
                                        OptionPrice = optionValues.AdditionalCost,
                                        OptionId = options.OptionId.ToString(),
                                        OptionTypeId = optionValues.OptionTypeId.ToString(),
                                        OptionName = options.OptionName ?? "",
                                        OptionTypeName = optionValues.OptionType ?? "",
                                        WindowOptionId = windowOptions.WindowoptionId,
                                        OptionMarkup = options.Markup ?? 0.0m
                                    }).ToList();
            }
            else
            {
                optionTaxDetails = (from windowOptions in dbContext.WindowOptions
                                    join optionValues in dbContext.TblOptionsTypes on windowOptions.OptionTypeId equals optionValues.OptionTypeId
                                    join options in dbContext.TblOptions on optionValues.OptionId equals options.OptionId
                                    join taxDetails in dbContext.TblTaxRates.Where(x => x.TenantId == claimsProperty.GetTenantId()) on options.TaxId equals taxDetails.TaxRateId into taxJoin
                                    from tax in taxJoin.DefaultIfEmpty()
                                    where options.IsActive == true
                                          && windowOptions.TenantId == claimsProperty.GetTenantId()
                                          && windowOptions.WindowId == product.WindowId
                                          && options.ProductId == product.ProductId
                                          && windowOptions.IsActive == true
                                    select new OptionTaxVM
                                    {
                                        TaxId = (tax != null && tax.TaxRateId != Guid.Empty ? tax.TaxRateId : product.TaxRateId) ?? Guid.Empty,
                                        TaxName = (tax.TaxRateId != Guid.Empty ? tax.TaxName : product.TaxName) ?? "",
                                        MunicipalityTax = (tax.TaxRateId != Guid.Empty ? tax.Municipality : product.MunicipalityTax) ?? 0.0m,
                                        StateTax = (tax.TaxRateId != Guid.Empty ? tax.State : product.StateTax) ?? 0.0m,
                                        CityTax = (tax.TaxRateId != Guid.Empty ? tax.City : product.CityTax) ?? 0.0m,
                                        OptionPrice = optionValues.AdditionalCost,
                                        OptionId = options.OptionId.ToString(),
                                        OptionTypeId = optionValues.OptionTypeId.ToString(),
                                        OptionName = options.OptionName ?? "",
                                        OptionTypeName = optionValues.OptionType ?? "",
                                        WindowOptionId = windowOptions.WindowoptionId,
                                        OptionMarkup = options.Markup,
                                    }).ToList();
            }

            return optionTaxDetails;
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
