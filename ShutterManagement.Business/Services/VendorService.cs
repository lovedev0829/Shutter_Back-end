using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class VendorService : BaseRepository<ShutterDbContext, TblVendor>, IVendorService
    {
        public Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<IResponseVm<string>> Create_Vendor(CreateComponent model)
        {
            throw new NotImplementedException();
        }

        public Task<IResponseVm<string>> Delete(Guid Id)
        {
            throw new NotImplementedException();
        }

        public async Task<IResponseVm<dynamic>> GetAll_Vendor(Pagination filter)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var IsComponentExists = (await GetAll())
                    .Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId)
                    .ToList();

                var FilterRecords = IsComponentExists
                    .Where(x => string.IsNullOrEmpty(filter.SearchValue) ||
                                (x.VendorName ?? "").Contains(filter.SearchValue.Trim()) ||
                                (x.Notes ?? "").Contains(filter.SearchValue.Trim()) ||
                                (x.ContactInformation ?? "").Contains(filter.SearchValue.Trim()))
                    .ToList();

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

            return await Task.FromResult(responseVm);
        }

        public Task<IResponseVm<dynamic>> Update_Vendor(UpdateComponent model)
        {
            throw new NotImplementedException();
        }
    }
}
