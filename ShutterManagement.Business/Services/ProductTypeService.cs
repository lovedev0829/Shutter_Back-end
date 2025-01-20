using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class ProductTypeService : BaseRepository<ShutterDBContext, MstProductType>, IMstProductType
    {
        private readonly ShutterDBContext _context;
       

        public ProductTypeService(ShutterDBContext context)
        {
            _context = context;
        }

        public async Task<IResponseVm<List<ProductTypeList>>> GetAll_ProductType()
        {
            IResponseVm<List<ProductTypeList>> responseVm = new ResponseVm<List<ProductTypeList>>();
            try
            {
                var ProductTypeList = (await GetAll()).Where(x => x.IsDelete == false).ToList();

                    responseVm.Data = ProductTypeList.Select(x => new ProductTypeList
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        IsActive = x.IsActive,
                    }).ToList();
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                    responseVm.TotalRecords = ProductTypeList.Count;

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
         }
    }
}
