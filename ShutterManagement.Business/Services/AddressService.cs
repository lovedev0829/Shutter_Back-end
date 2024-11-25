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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ShutterManagement.Business.Services
{


    public class AddressService : BaseRepository<ShutterDbContext, TblAddress>, IAddress
    {
        private readonly ShutterDbContext _context;
        private readonly ClaimsProperty claimsProperty;
        private readonly IContactService contactService;

        public AddressService(ShutterDbContext context, ClaimsProperty claimsProperty, IContactService contactService)
        {
            _context = context;
            this.claimsProperty = claimsProperty;
            this.contactService = contactService;
        }



        public async Task<IResponseVm<string>> Create_Address(CreateAddress model)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsContactExists = await contactService.AnyAsync(x => x.ContactId.ToString() == model.ContactId
                 && x.TenantId == AppConstant.LoggedInTenantId);
                if (IsContactExists)
                {
                    var address = new TblAddress
                    {
                        ContactId = Guid.Parse(model.ContactId),
                        Label = model.Label,
                        StreetAddress = model.StreetAddress,
                        City = model.City,
                        State = model.State,
                        ZipCode = model.ZipCode,
                        Notes = model.Notes,
                        AccessCode = model.AccessCode,
                        FullName = model.FullName,
                        Email = model.Email,
                        Phone = model.Phone,
                        IsActive = true,
                        IsDelete = false,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID,
                        TenantId = AppConstant.LoggedInTenantId
                    };

                    var res = await Add(address);
                    if (res.AddressId != Guid.Empty)
                    {
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.InsertSuccessMsg;
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.NotRelatesToTenant;
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
                var IsContactExists = await contactService.AnyAsync(x => x.ContactId.ToString() == model.ContactId
                 && x.TenantId == AppConstant.LoggedInTenantId);
                if (IsContactExists)
                {
                    var AddressDetails = GetAllQuerable().Where(x => x.AddressId.ToString() == model.AddressId && x.ContactId.ToString() == model.ContactId && x.TenantId == AppConstant.LoggedInTenantId
                    && x.IsDelete == false).FirstOrDefault();

                    if (AddressDetails != null)
                    {
                        AddressDetails.Label = model.Label;
                        AddressDetails.StreetAddress = model.StreetAddress;
                        AddressDetails.Notes = model.Notes;
                        AddressDetails.City = model.City;
                        AddressDetails.State = model.State;
                        AddressDetails.ZipCode = model.ZipCode;
                        AddressDetails.FullName = model.FullName;
                        AddressDetails.AccessCode = model.AccessCode;
                        AddressDetails.Phone = model.Phone;
                        AddressDetails.Email = model.Email;
                        AddressDetails.ModifiedBy = AppConstant.LoggedInUserID;
                        AddressDetails.UpdatedAt = DateTime.Now;

                        await Update(AddressDetails);

                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.NotRelatesToTenant;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;

        }

        public async Task<IResponseVm<string>> DeleteAddress(Guid AddressId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var AddressDetails = GetAllQuerable().Where(x => x.AddressId == AddressId && x.TenantId == AppConstant.LoggedInTenantId
                && x.IsDelete == false).FirstOrDefault();

                if (AddressDetails != null)
                {
                    AddressDetails.IsDelete = true;
                    AddressDetails.IsActive = false;
                    AddressDetails.ModifiedBy = AppConstant.LoggedInUserID;
                    AddressDetails.UpdatedAt = DateTime.Now;

                    await Update(AddressDetails);

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
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

        public async Task<IResponseVm<string>> ActiveInActiveAddress(Guid AddressId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var AddressDetails = GetAllQuerable().Where(x => x.AddressId == AddressId && x.TenantId == AppConstant.LoggedInTenantId
                && x.IsDelete == false).FirstOrDefault();

                if (AddressDetails != null)
                {
                    AddressDetails.IsActive = !AddressDetails.IsActive;
                    AddressDetails.ModifiedBy = AppConstant.LoggedInUserID;
                    AddressDetails.UpdatedAt = DateTime.Now;

                    await Update(AddressDetails);

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
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

        public async Task<IResponseVm<List<GetAllAddress>>> GetAll_Address(Pagination filter)
        {
            IResponseVm<List<GetAllAddress>> responseVm = new ResponseVm<List<GetAllAddress>>();
            try
            {
                var AddressList = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).ToList();
                filter.SearchValue = filter.SearchValue.Trim().ToLower();
                var FilterRecords = AddressList
                                    .Where(x =>
                                    (filter.SearchValue != "" ?
                                    ((x.StreetAddress ?? "").ToLower().Contains(filter.SearchValue.Trim())
                                    || (x.AccessCode ?? "").ToLower().Contains(filter.SearchValue.Trim())
                                    || (x.Label ?? "").ToLower().Contains(filter.SearchValue.Trim())
                                    || (x.ZipCode ?? "").Contains(filter.SearchValue.Trim())
                                    || (x.City ?? "").ToLower().Contains(filter.SearchValue.Trim())
                                    || (x.State ?? "").ToLower().Contains(filter.SearchValue.Trim())
                                    || (x.FullName ?? "").ToLower().Contains(filter.SearchValue.Trim())
                                    || (x.Phone ?? "").ToLower().Contains(filter.SearchValue.Trim())
                                    || (x.Email ?? "").ToLower().Contains(filter.SearchValue.Trim())
                                    ) : true)).ToList();
                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();



                responseVm.Data = TakenRecords.Select(x => new GetAllAddress
                {
                    AddressId = x.AddressId,
                    ContactId = x.ContactId.ToString(),
                    Label = x.Label,
                    StreetAddress = x.StreetAddress,
                    Notes = x.Notes,
                    City = x.City,
                    State = x.State,
                    ZipCode = x.ZipCode,
                    AccessCode = x.AccessCode,
                    FullName = x.FullName,
                    Email = x.Email,
                    Phone = x.Phone,
                    IsActive = x.IsActive,
                }).ToList();
                responseVm.IsSuccess = true;
                responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
                responseVm.TotalRecords = TakenRecords.Count;


            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        //public Task<IResponseVm<AddressResponse>> GetById_Address(Guid AddressId)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<IResponseVm<AddressResponse>> GetById_Address(Guid id)
        //{
        //    IResponseVm<AddressResponse> responseVm = new ResponseVm<AddressResponse>();
        //    try
        //    {
        //        var address = await GetAllQuerable() 
        //            .Where(c => c.AddressId == id && c.IsActive == true && c.TenantId == AppConstant.LoggedInTenantId)
        //            .Select(c => new AddressResponse
        //            {
        //                ContactId = c.ContactId,
        //                Label = c.Label,
        //                AddressLine1 = c.AddressLine1,
        //                AddressLine2 = c.AddressLine2,
        //                City = c.City,
        //                State = c.State,
        //                ZipCode = c.ZipCode,
        //                Country = c.Country,

        //            })
        //            .FirstOrDefaultAsync();  

        //        if (address != null)
        //        {
        //            responseVm.IsSuccess = true;
        //            responseVm.Message = $"{AppConstant.FetchSuccessMsg}";
        //            responseVm.Data = address;
        //            responseVm.TotalRecords = 1;
        //        }
        //        else
        //        {
        //            responseVm.Message = AppConstant.NoDataFound;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.Message = AppConstant.ExceptionError + ex.Message;
        //    }
        //    return responseVm;
        //}

    }
}
