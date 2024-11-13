using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.SPModel;
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

namespace ShutterManagement.Business.Services
{


    public class AddressService : BaseRepository<ShutterDbContext, TblAddress>, IAddress
      {
       private readonly ShutterDbContext _context;
        private readonly ClaimsProperty claimsProperty;

        public AddressService(ShutterDbContext context,ClaimsProperty claimsProperty)
      {
        _context = context;
        this.claimsProperty = claimsProperty;
        }

      public Task<IResponseVm<string>> ActiveInActiveAddress(Guid id)
      {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var address = GetAllQuerable()
                    .Where(c => c.AddressId == id && c.TenantId == AppConstant.LoggedInTenantId)
                    .FirstOrDefault();

                if (address != null)
                {

                    address.IsActive = false;
                    address.ModifiedBy = AppConstant.LoggedInUserID;
                    address.UpdatedAt = DateTime.Now;
                    Update(address);

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InactivationMsg;
                }
                else
                {
                    responseVm.Message = AppConstant.NoUserFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return Task.FromResult(responseVm);

        }

        public async Task<IResponseVm<string>> Create_Address(CreateAddress model)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var address = new TblAddress
                {
                    ContactId = model.ContactId,
                    Label = model.Label,
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    City = model.City,
                    State = model.State,
                    ZipCode = model.ZipCode,
                    Country = model.Country,
                    CreatedAt = DateTime.Now,
                    CreatedBy = AppConstant.LoggedInUserID,
                    IsActive = true,
                    IsDelete = false,
                    TenantId = AppConstant.LoggedInTenantId
                };
                if (address != null)
                {
                    _context.TblAddresses.Add(address);
                    await _context.SaveChangesAsync();
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                    responseVm.TotalRecords = 1;
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


        public Task<IResponseVm<string>> DeleteAddress(Guid id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            { 
                var address = GetAllQuerable()
                    .Where(c => c.AddressId == id && c.TenantId == AppConstant.LoggedInTenantId)
                    .FirstOrDefault();

                if (address != null)
                {

                    address.IsActive = false;
                    address.IsDelete = true;
                    address.ModifiedBy = AppConstant.LoggedInUserID;
                    address.UpdatedAt = DateTime.Now;
                    Update(address);

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.DeleteSuccessMsg;
                }
                else
                {
                    responseVm.Message = AppConstant.NoUserFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return Task.FromResult(responseVm);
        }



        public async Task<IResponseVm<List<AddressResponse>>> GetAll_Address()
        {
            IResponseVm<List<AddressResponse>> responseVm = new ResponseVm<List<AddressResponse>>();
            try
            {
                var addresses = await GetAllQuerable()
                    .Where(c => c.IsActive == true && c.TenantId == AppConstant.LoggedInTenantId)
                    .Select(c => new AddressResponse
                    {
                        AddressId = c.AddressId,
                        ContactId = c.ContactId,
                        Label = c.Label,
                        AddressLine1 = c.AddressLine1,
                        AddressLine2 = c.AddressLine2,
                        City = c.City,
                        State = c.State,
                        ZipCode = c.ZipCode,
                        Country = c.Country,
                    })
                    .ToListAsync();

                if (addresses != null)  
                {
                    responseVm.IsSuccess = true;
                    responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
                    responseVm.Data = addresses;
                    responseVm.TotalRecords = addresses.Count;
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "No addresses found.";
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;  
        }

        public async Task<IResponseVm<AddressResponse>> GetById_Address(Guid id)
        {
            IResponseVm<AddressResponse> responseVm = new ResponseVm<AddressResponse>();
            try
            {
                var address = await GetAllQuerable() 
                    .Where(c => c.AddressId == id && c.IsActive == true && c.TenantId == AppConstant.LoggedInTenantId)
                    .Select(c => new AddressResponse
                    {
                        ContactId = c.ContactId,
                        Label = c.Label,
                        AddressLine1 = c.AddressLine1,
                        AddressLine2 = c.AddressLine2,
                        City = c.City,
                        State = c.State,
                        ZipCode = c.ZipCode,
                        Country = c.Country,
                    })
                    .FirstOrDefaultAsync();  

                if (address != null)
                {
                    responseVm.IsSuccess = true;
                    responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
                    responseVm.Data = address;
                    responseVm.TotalRecords = 1;
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


        public async Task<IResponseVm<AddressResponse>> Update_Address(UpdateAddress model)
        {
            IResponseVm<AddressResponse> responseVm = new ResponseVm<AddressResponse>();
            try
            {
                var address = GetAllQuerable().Where(c => c.AddressId == model.AddressId && c.IsActive == true && c.TenantId == AppConstant.LoggedInTenantId)
                    .FirstOrDefault();

                if (address != null)
                {
                    address.AddressLine1 = model.AddressLine1;
                    address.AddressLine2 = model.AddressLine2;
                    address.City = model.City;
                    address.State = model.State;
                    address.ZipCode = model.ZipCode;
                    address.Country = model.Country;
                    address.ModifiedBy = AppConstant.LoggedInUserID;
                    address.UpdatedAt= DateTime.Now;
                    await Update(address);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                    responseVm.TotalRecords = 1;
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
