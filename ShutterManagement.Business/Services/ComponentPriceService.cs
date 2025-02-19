using Microsoft.Extensions.Options;
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
                    responseVm.Data = ChipData;
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
                  where estimate.EstimateId.ToString() == EstimateID
                  join estwindowProduct in dbContext.TblEstimateWindowProducts on estimatewindow.WindowId equals estwindowProduct.WindowId
                  where estimatewindow.EstimateId.ToString() == EstimateID
                  join estwinprocomp in dbContext.TblEstimateWindowProductComponents on estwindowProduct.ProductId equals estwinprocomp.ProductId
                  where estwinprocomp.EstimateId.ToString() == EstimateID  && estwindowProduct.WindowId == estwinprocomp.WindowId
                  join productComponentmat in dbContext.TblEstimateWindowProductComponentMaterials on estwinprocomp.ComponentId equals productComponentmat.ComponentId
                  where productComponentmat.EstimateId.ToString() == EstimateID && estwindowProduct.WindowId == estwinprocomp.WindowId
                  where estwindowProduct.WindowProductId == WindowProductId
                  && estimate.IsActive == true && estimate.IsDelete == false
                  && estimatewindow.IsActive == true && estimatewindow.IsDelete == false
                  && estwindowProduct.IsActive == true && estwindowProduct.IsDelete == false
                  && estwinprocomp.IsActive == true && estwinprocomp.IsDelete == false
                  && productComponentmat.IsActive == true && productComponentmat.IsDelete == false
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
                      ProductTypeName = estwinprocomp.ProductTypeName,

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
                  join productType in dbContext.MstProductTypes on product.ProductType equals productType.ProductId.ToString()
                  join productComponent in dbContext.TblProductComponents on product.ProductId equals productComponent.ProductId
                  join expression in dbContext.TblExpressions on productComponent.PricingExpression equals expression.Id.ToString()
                  join component in dbContext.TblComponents on productComponent.ComponentId equals component.ComponentId
                  join componentMaterial in dbContext.TblComponentMaterials on component.ComponentId equals componentMaterial.ComponentId
                  join material in dbContext.TblRawMaterials on componentMaterial.RawMaterialId equals material.RawMaterialId
                  where windowProduct.WindowProductId == WindowProductId
                  && window.IsActive == true && window.IsDelete == false
                  && windowProduct.IsActive == true && windowProduct.IsDelete == false
                  && product.IsActive == true && product.IsDelete == false
                  && productType.IsActive == true && productType.IsDelete == false
                  && productComponent.IsActive == true && productComponent.IsDelete == false
                  && component.IsActive == true && component.IsDelete == false
                  && expression.IsActive == true && expression.IsDelete == false
                  && componentMaterial.IsActive == true && componentMaterial.IsDelete == false
                  && material.IsActive == true && material.IsDelete == false
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
                      IsCutSheet = material.IsCutSheet
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
    }
}
