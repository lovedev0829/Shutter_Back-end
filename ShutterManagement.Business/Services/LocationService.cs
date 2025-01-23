using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class LocationService : BaseRepository<BtprojecShuttermanagementDevContext, TblLocation>, ILocationService
    {
        private readonly ITenantService tenantService;

        public LocationService(ITenantService tenantService)
        {
            this.tenantService = tenantService;
        }
        public async Task<IResponseVm<string>> Create_Location(CreateLocation model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                //bool isTenant = await tenantService.AnyAsync(x =>
                //    x.TenantId == AppConstant.LoggedInTenantId &&
                //    x.IsDelete == false &&
                //    x.IsActive == true);
                //if (!isTenant)
                //{
                //    responseVm.Message = AppConstant.TenantNotFound;
                //    responseVm.IsSuccess = false;
                //    return responseVm;
                //}
                var isDuplicateLocation = GetAllQuerable().Where
                    (x => x.LocationName == model.LocationName &&
                                   x.IsDelete == false);

                if (isDuplicateLocation != null && isDuplicateLocation.Any())
                {
                    responseVm.Message = AppConstant.AlreadyExists;
                    responseVm.IsSuccess = false;
                    return responseVm;
                }
                var location = new TblLocation
                {
                    LocationId = Guid.NewGuid(),
                    LocationName = model.LocationName?.Trim(),
                    TenantId = AppConstant.LoggedInTenantId,
                    CreatedAt = DateTime.Now,
                    IsDelete = false,
                    IsActive = true
                };
                var res = Add(location);
                responseVm.Message = AppConstant.InsertSuccessMsg;
                responseVm.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError;
                responseVm.IsSuccess = false;
            }
            return responseVm;
        }

        public async Task<IResponseVm<List<LocationVm>>> GetAll_Location()
        {
            IResponseVm<List<LocationVm>> responseVm = new ResponseVm<List<LocationVm>>();
            try
            {
                var locations = dbContext.TblLocations
                    .Where(x=> x.IsDelete == false)
                    .OrderBy(x => x.LocationName)
                    .Select(x => new LocationVm
                    {
                        LocationId = x.LocationId,
                        LocationName = x.LocationName,
                        IsActive = x.IsActive
                    })
                    .ToList();

                responseVm.Message = AppConstant.FetchSuccessMsg;
                responseVm.IsSuccess = true;
                responseVm.Data = locations;
                responseVm.TotalRecords = locations.Count;
            }
            catch (Exception ex)
            {
                responseVm.Message = ex.Message + AppConstant.ExceptionError;
                responseVm.IsSuccess = false;

            }

            return responseVm;
        }



    }
}
