using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;

namespace ShutterManagement.Business.Services
{
    public class ManagePartnersProductsService : BaseRepository<ShutterDBContext, TblManagePartnersProduct>, IManagePartnersProducts
    {
        private readonly ITenantService tenantService;
        private readonly IManagePartnersProductsOptions managePartnersProductsOptions;

        public ManagePartnersProductsService(ITenantService tenantService,IManagePartnersProductsOptions managePartnersProductsOptions)
        {
            this.tenantService = tenantService;
            this.managePartnersProductsOptions = managePartnersProductsOptions;
        }

        public async Task<IResponseVm<string>> CreateManagePartnerProduct(CreateManagePartnerProduct model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.ParentTenantId == null);
                if (IsTenant)
                {
                    TblManagePartnersProduct managePartnersProduct = new TblManagePartnersProduct
                    {
                        ManageProductId = Guid.NewGuid(),
                        ProductId = model.ProductId,
                        IsAvailable = model.IsAvailable,
                        WholeSalePrice = model.WholeSalePrice,
                        MaxPartnerPrice = model.MaxPartnerPrice,
                        PartnerSellingPrice = model.PartnerSellingPrice,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID,
                        TenantId = Guid.Parse(model.TenantId),
                        IsActive = true,
                        IsDelete = false
                    };
                    await Add(managePartnersProduct);
                    if (managePartnersProduct.ManageProductId != Guid.Empty)
                    {
                        foreach (var item in model.ManagePartnerProductOptions)
                        {
                            //var IsOptionTypeExists = await optionsTypeService.AnyAsync(x => x.IsActive == true
                            //&& x.IsDelete == false && x.OptionId == options.OptionId && x.OptionType == item.OptionType);
                            //if (!IsOptionTypeExists)
                            //{
                            TblManagePartnersProductsOption optionsType = new TblManagePartnersProductsOption
                            {
                                OptionId = item.OptionId,
                                OptionTypeId = item.OptionTypeId,
                                IsEnable = item.IsEnable,
                                WholeSalePrice = item.WholeSalePrice,
                                MaxPrice = item.MaxPrice,
                                CreatedAt = DateTime.Now,
                                CreatedBy = AppConstant.LoggedInUserID,
                                IsActive = true,
                                IsDelete = false
                            };
                            await managePartnersProductsOptions.Add(optionsType);
                            //}
                            //else
                            //{
                            //    alloptiontypeadded = false;
                            //    message += $"{item.OptionType} OptionType " + AppConstant.AlreadyExists;
                            //}
                        }
                        //if (!alloptiontypeadded)
                        //{
                        //    responseVm.IsSuccess = true;
                        //    responseVm.Message = message;
                        //}
                        //else
                        //{
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.InsertSuccessMsg;
                        //}
                    }

                }
                else
                {
                    responseVm.Message = AppConstant.AlreadyExists;
                }
            }
            //    }
            //    else
            //    {
            //        responseVm.Message = AppConstant.AccessDenied;
            //    }
            //}
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
            }
    }
}
