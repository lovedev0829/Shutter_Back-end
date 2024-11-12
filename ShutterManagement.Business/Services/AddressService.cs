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

      public AddressService(ShutterDbContext context)
      {
        _context = context;
       }

      public Task<IResponseVm<string>> ActiveInActiveAddress(Guid id)
      {
         throw new NotImplementedException();
      }

      public async Task Create_Address(CreateAddress model)
        {
            var address = new TblAddress
            {
                ContactId = model.ContactId,
                Label = model.Label,
                AddressLine1= model.AddressLine1,
                AddressLine2 = model.AddressLine2,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                Country = model.Country,

            };

            _context.TblAddresses.Add(address);
            await _context.SaveChangesAsync();
        }

        public Task<IResponseVm<string>> DeleteAddress(Guid id)
        {
            throw new NotImplementedException();
        }

        //public Task<IResponseVm<string>> DeleteAddress(Guid id)
        //{

        //    IResponseVm<string> responseVm = new ResponseVm<string>();
        //    try
        //    {
        //        var address = GetAllQuerable()
        //            .Where(c => c.ContactId == id
        //            && c.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();

        //        if (contact != null)
        //        {
        //            contact.IsActive = false;
        //            contact.IsDelete = true;
        //            await Update(contact);

        //            responseVm.IsSuccess = true;
        //            responseVm.Message = AppConstant.DeleteSuccessMsg;
        //        }
        //        else
        //        {
        //            responseVm.Message = AppConstant.NoUserFound;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.Message = AppConstant.ExceptionError + ex.Message;
        //    }
        //    return responseVm;
        //}

        //public Task<IResponseVm<string>> DeleteAddress(Guid id)
        //{
        //    IResponseVm<string> responseVm = new ResponseVm<string>();

        //    try
        //    {
        //        var address = GetAllQuerable()
        //            .Where(c => c.AddressId == AddressId
        //            && c.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();

        //        if (contact != null)
        //        {
        //            contact.IsActive = false;
        //            contact.IsDelete = true;
        //            await Update(contact);

        //            responseVm.IsSuccess = true;
        //            responseVm.Message = AppConstant.DeleteSuccessMsg;
        //        }
        //        else
        //        {
        //            responseVm.Message = AppConstant.NoUserFound;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.Message = AppConstant.ExceptionError + ex.Message;
        //    }
        //    return responseVm;
        //}

        public Task<List<AddressResponse>> GetAll_Address()
        {

            var addresses = _context.TblAddresses
                .Where(c => c.IsActive == true)
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

            return addresses;
        }

        public async Task<AddressResponse> GetById_Address(Guid id)
        {
            var address = await _context.TblAddresses
                .Where(c => c.AddressId == id && c.IsActive == true)
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

            return address;
        }

        public async Task Update_Address(CreateAddress model)
        {
           var address = await _context.TblAddresses
                .FirstOrDefaultAsync(c => c.AddressId == model.AddressId);

            if (address == null)
            {
                address.AddressLine1 = model.AddressLine1;
                address.AddressLine2 = model.AddressLine2;
                address.City = model.City;
                address.State = model.State;
                address.ZipCode = model.ZipCode;
                address.Country = model.Country;
            }

            await _context.SaveChangesAsync();
        }

       
    }
}
