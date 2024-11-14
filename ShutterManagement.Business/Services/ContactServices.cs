using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShutterManagement.Core;
using ShutterManagement.Services;
using Azure;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Business.Services
{
    public class ContactServices :BaseRepository<ShutterDbContext,TblContact> ,IContactService
    {
        private readonly ShutterDbContext _context;
        private readonly ITenantService tenantService;
        private readonly ClaimsProperty claimsProperty;

        public ContactServices(ShutterDbContext context,ITenantService tenantService,ClaimsProperty claimsProperty)
        {
            _context = context;
            this.tenantService = tenantService;
            this.claimsProperty = claimsProperty;
        }

        public async Task<IResponseVm<Sp_Contact>> Create_Contact(Sp_Contact model)
        {
            IResponseVm<Sp_Contact> responseVm = new ResponseVm<Sp_Contact>();
            try
            {
                var IsContactExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId); 
                if (IsContactExists != null)
                {
                    if (!model.IsBillingAddressSameAsAddress)
                    {
                        if (model.billingAddressVm == null ||
                            string.IsNullOrEmpty(model.billingAddressVm.Label) ||
                            string.IsNullOrEmpty(model.billingAddressVm.AddressLine1) ||
                            string.IsNullOrEmpty(model.billingAddressVm.City) ||
                            string.IsNullOrEmpty(model.billingAddressVm.State) ||
                            string.IsNullOrEmpty(model.billingAddressVm.ZipCode) ||
                            string.IsNullOrEmpty(model.billingAddressVm.Country))
                        {
                            string missingFields = "";

                            if (string.IsNullOrEmpty(model.billingAddressVm.Label)) missingFields += "Label, ";
                            if (string.IsNullOrEmpty(model.billingAddressVm.AddressLine1)) missingFields += "Address Line 1, ";
                            if (string.IsNullOrEmpty(model.billingAddressVm.City)) missingFields += "City, ";
                            if (string.IsNullOrEmpty(model.billingAddressVm.State)) missingFields += "State, ";
                            if (string.IsNullOrEmpty(model.billingAddressVm.ZipCode)) missingFields += "Zip Code, ";
                            if (string.IsNullOrEmpty(model.billingAddressVm.Country)) missingFields += "Country, ";

                            missingFields = missingFields.TrimEnd(',', ' ');

                            responseVm.Message = $"Billing address information is incomplete. The following fields are required: {missingFields}.";
                        }
                    }

                    var contact = new TblContact
                    {
                        ContactType = model.ContactType,
                        Name = model.Name,
                        PhoneNumber = model.PhoneNumber,
                        Email = model.Email,
                        Website = model.Website,
                        Notes = model.Notes,
                        IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID,
                        TenantId = AppConstant.LoggedInTenantId,

                        TblContactBillingAddresses = model.IsBillingAddressSameAsAddress
                            ? new List<TblContactBillingAddress>()
                            : new List<TblContactBillingAddress>
                            {
                new TblContactBillingAddress
                {
                    Label = model.billingAddressVm?.Label,
                    AddressLine1 = model.billingAddressVm?.AddressLine1,
                    AddressLine2 = model.billingAddressVm?.AddressLine2,
                    City = model.billingAddressVm?.City,
                    State = model.billingAddressVm?.State,
                    ZipCode = model.billingAddressVm?.ZipCode,
                    Country = model.billingAddressVm?.Country
                }
                }
                };

                    if (contact != null)
                    {
                        try
                        {
                            var res = await Add(contact);
                            responseVm.IsSuccess = true;
                            responseVm.Message = $"{AppConstant.InsertSuccessMsg}";
                        }
                        catch (Exception ex)
                        {
                            responseVm.IsSuccess = false;
                            responseVm.Message = AppConstant.ExceptionError + ex.Message;
                        }
                        
                       
                        
                    }
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"{AppConstant.AlreadyExists}";
                }

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> ActiveInActiveContact(Guid id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var contact = GetAllQuerable()
                    .Where(c => c.ContactId == id
                    && c.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();

                if (contact != null)
                {
                    contact.IsActive = !contact.IsActive;
                    await Update(contact);

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
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
            return responseVm;
        }
        public async Task<IResponseVm<string>> DeleteContact(Guid id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var contact = GetAllQuerable()
                    .Where(c => c.ContactId == id
                    && c.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();

                if (contact != null)
                {
                    contact.IsActive = false;
                    contact.IsDelete = true;
                    await Update(contact);

                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.DeleteSuccessMsg;
                }
                else
                {
                    responseVm.Message = AppConstant.NoUserFound;
                }
            }
            catch (Exception ex) {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<dynamic> GetAll_Contact(Pagination filter)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var ContactList = new List<TblContact>();
                if (filter.TenantId != "")
                {
                    var IsRealtedTenant = tenantService.GetAllQuerable().Where(x => x.TenantId.ToString() == filter.TenantId && x.ParentTenantId == AppConstant.LoggedInTenantId).FirstOrDefault();
                    if (IsRealtedTenant == null)
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                        return responseVm;
                    }
                    else
                    {
                        ContactList = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId.ToString() == filter.TenantId).ToList();
                    }
                }
                else
                {
                    ContactList = (await GetAll()).Where(x => x.IsDelete == false && x.TenantId == AppConstant.LoggedInTenantId).ToList();
                }
                var FilterRecords = ContactList
                                    .Where(x =>
                                    (filter.SearchValue != "" ?
                                    ((x.Notes?? "").Contains(filter.SearchValue)
                                    || (x.Email ?? "").Contains(filter.SearchValue)
                                    || (x.Website ?? "").Contains(filter.SearchValue) 
                                    || (x.PhoneNumber ?? "").Contains(filter.SearchValue)
                                    || (x.Name ?? "").Contains(filter.SearchValue)
                                    ) : true)).ToList();
                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();
                if (TakenRecords != null && TakenRecords.Count > 0)
                {
                    responseVm.Data = TakenRecords;
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                    responseVm.TotalRecords = FilterRecords.Count;
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

        public async Task Update_Contact(Sp_Contact model)
        {
            IResponseVm<Sp_Contact> responseVm = new ResponseVm<Sp_Contact>();
            try
            {
                var IsContactExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId);

                if (IsContactExists)
                {
                    var contact = await GetAllQuerable()
                        .FirstOrDefaultAsync(c => c.ContactId == model.ContactId);

                    if (contact != null)
                    {
                        contact.ContactType = model.ContactType;
                        contact.Name = model.Name;
                        contact.PhoneNumber = model.PhoneNumber;
                        contact.Email = model.Email;
                        contact.Website = model.Website;
                        contact.Notes = model.Notes;
                        contact.IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress;
                        contact.IsActive = true;

                        if (!model.IsBillingAddressSameAsAddress)
                        {
                            contact.TblContactBillingAddresses = new List<TblContactBillingAddress>
                    {
                        new TblContactBillingAddress
                        {
                            Label = model.billingAddressVm?.Label,
                            AddressLine1 = model.billingAddressVm?.AddressLine1,
                            AddressLine2 = model.billingAddressVm?.AddressLine2,
                            City = model.billingAddressVm?.City,
                            State = model.billingAddressVm?.State,
                            ZipCode = model.billingAddressVm?.ZipCode,
                            Country = model.billingAddressVm?.Country
                        }
                    };
                        }

                        Update(contact);
                    }
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            }

        }
    }
