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

namespace ShutterManagement.Business.Services
{
    public class ContactServices :BaseRepository<ShutterDbContext,TblContact> ,IContactService
    {
        private readonly ShutterDbContext _context;

        public ContactServices(ShutterDbContext context)
        {
            _context = context;
        }

        public async Task Create_Contact(Sp_Contact model)
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

                    throw new ArgumentException($"Billing address information is incomplete. The following fields are required: {missingFields}.");
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

            _context.TblContacts.Add(contact);
            await _context.SaveChangesAsync();
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

        public async Task<List<Sp_Contact>> GetAll_Contact()
        {
            var contacts = await _context.TblContacts
                .Where(c => c.IsActive == true)
                .Select(c => new Sp_Contact
                {
                    ContactId = c.ContactId,
                    ContactType = c.ContactType,
                    Name = c.Name,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    Website = c.Website,
                    Notes = c.Notes,
                    IsBillingAddressSameAsAddress = c.IsBillingAddressSameAsAddress
                })
                .ToListAsync();

            return contacts;
        }

        public async Task<Sp_Contact> GetById_Contact(Guid id)
        {
            var contact = await _context.TblContacts
                .Where(c => c.ContactId == id)
                .Select(c => new Sp_Contact
                {
                    ContactId = c.ContactId,
                    ContactType = c.ContactType,
                    Name = c.Name,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    Website = c.Website,
                    Notes = c.Notes,
                    IsBillingAddressSameAsAddress = c.IsBillingAddressSameAsAddress
                })
                .FirstOrDefaultAsync();

            return contact;
        }

        public async Task Update_Contact(Sp_Contact model)
        {
            var contact = await _context.TblContacts
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

                await _context.SaveChangesAsync();
            }

            //public async Task<IResponseVm<string>> ContactActiveInactive(Guid contactId)
            //{
            //    IResponseVm<string> responseVm = new ResponseVm<string>();

            //    try
            //    {
            //        var contact = await _context.TblContacts
            //            .Include(c => c.TblContactRoles)      
            //            .Include(c => c.TblContactBillingAddresses) 
            //            .FirstOrDefaultAsync(c => c.ContactId == contactId);

            //        if (contact != null)
            //        {
            //            contact.IsActive = !(contact.IsActive ?? true); 

            //            foreach (var user in contact.TblContactRoles)  
            //            {
            //                user.IsActive = !user.IsActive;  
            //                user.UpdatedAt = DateTime.Now; 
            //            }

            //            if (!contact.IsBillingAddressSameAsAddress && contact.TblContactBillingAddresses.Any())
            //            {
            //                foreach (var billingAddress in contact.TblContactBillingAddresses)
            //                {
            //                }
            //            }

            //            await _context.SaveChangesAsync();

            //            responseVm.IsSuccess = true;
            //            responseVm.Message = "Contact and related entities status updated successfully.";
            //        }
            //        else
            //        {
            //            responseVm.IsSuccess = false;
            //            responseVm.Message = "Contact not found.";
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        responseVm.IsSuccess = false;
            //        responseVm.Message = $"Error toggling active status: {ex.Message}";
            //    }

            //    return responseVm;
            //}

        }
    }
}
