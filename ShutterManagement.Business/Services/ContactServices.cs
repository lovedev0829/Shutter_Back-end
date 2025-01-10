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
using System.Diagnostics.Metrics;
using System.Reflection.Emit;
using ShutterManagement.Core.SPModel.SP_Responses;
using Microsoft.IdentityModel.Tokens;
using ShutterManagement.Core.ViewModels.Response;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ShutterManagement.Core.Common;

namespace ShutterManagement.Business.Services
{
    public class ContactServices : BaseRepository<BtprojecShuttermanagementDevContext, TblContact>, IContactService
    {
        private readonly BtprojecShuttermanagementDevContext _context;
        private readonly IContactBillingService billingService;
        private readonly ITenantService tenantService;
        private readonly ITaxExemption taxExemptionService;
        private readonly ClaimsProperty claimsProperty;

        public ContactServices(BtprojecShuttermanagementDevContext context, IContactBillingService billingService,
            ITenantService tenantService, ITaxExemption taxExemptionService, ClaimsProperty claimsProperty)
        {
            _context = context;
            this.billingService = billingService;
            this.tenantService = tenantService;
            this.taxExemptionService = taxExemptionService;
            this.claimsProperty = claimsProperty;
        }

        public async Task<IResponseVm<string>> GetContactByEmailOrPhone(string email, string phone)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);

                if (IsTenantExists)
                {
                    var IsEmailExists = GetAllQuerable()
                        .Any(c => c.Email == email && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false);

                    var IsPhoneExists = GetAllQuerable()
                        .Any(c => c.PhoneNumber == phone && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false);

                    if (IsEmailExists && IsPhoneExists)
                    {
                        responseVm.Message = $"Email {email} and Phone {phone} {AppConstant.AlreadyExists}";
                    }
                    else if (IsEmailExists)
                    {
                        responseVm.Message = $"Email {email} {AppConstant.AlreadyExists}";
                    }
                    else if (IsPhoneExists)
                    {
                        responseVm.Message = $"Phone {phone} {AppConstant.AlreadyExists}";
                    }
                }
                else
                {
                    responseVm.Message = AppConstant.TenantNotFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Create_Contact(Sp_Contact model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);

                if (IsTenantExists)
                {
                    var IsContactExists = GetAllQuerable()
                        .Where(c => (c.Email == model.Email || c.PhoneNumber == model.PhoneNumber)
                        && c.TenantId == AppConstant.LoggedInTenantId
                        && c.IsDelete == false).ToList();
                    if (IsContactExists.Count > 0)
                    {
                        responseVm.Message = $"Email {model.Email} or Phone {model.PhoneNumber} " + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    if (!model.IsBillingAddressSameAsAddress && model.BillingAddressVm == null)
                    {
                        responseVm.Message = "Billing details required.";
                        return responseVm;
                    }
                    if(model.TaxExemptionVm.ExpiryDate < model.TaxExemptionVm.EffectiveDate)
                    {
                        responseVm.Message = "ExpiryDate must be greater than EffectiveDate.";
                        return responseVm;
                    }

                    var contactUniqueId =  GenerateUniqueId.GenerateUniqueIds(ModuleName.Cus, AppConstant.LoggedInCompanyCode);
                    TblContact CreateContact = new TblContact
                    {
                        ContactId = Guid.NewGuid(),
                        ContactType = model.ContactType,
                        Name = model.Name,
                        PhoneNumber = model.PhoneNumber,
                        Email = model.Email,
                        Website = model.Website,
                        Notes = model.Notes,
                        Referral = model.Referral,
                        ContactUniqueId = contactUniqueId,
                        IsTaxExemption = model.IsTaxExemption,
                        IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress,
                        CreatedBy = AppConstant.LoggedInUserID,
                        CreatedAt = DateTime.Now,
                        TenantId = AppConstant.LoggedInTenantId,
                        IsActive = true,
                        IsArchive = false,
                        IsDelete = false
                    };
                    var res = await Add(CreateContact);
                    if (res.ContactId != Guid.Empty)
                    {
                        if (!model.IsBillingAddressSameAsAddress)
                        {

                            TblContactBillingAddress CreateContactBillingAddress = new TblContactBillingAddress
                            {
                                ContactId = res.ContactId,
                                FullName = model.BillingAddressVm?.FullName,
                                PhoneNumber = model.BillingAddressVm?.PhoneNumber,
                                Email = model.BillingAddressVm?.Email,
                                City = model.BillingAddressVm?.City,
                                State = model.BillingAddressVm?.State,
                                ZipCode = model.BillingAddressVm?.ZipCode,
                                StreetAddress = model.BillingAddressVm?.StreetAddress,
                                CreatedAt = DateTime.Now,
                                IsActive = true,
                                IsDelete = false
                            };
                            await billingService.Add(CreateContactBillingAddress);
                        }
                        if (!model.IsTaxExemption)
                        {
                            string arrayString = string.Join(", ", model.TaxExemptionVm?.ExemptionCertificate);
                            TblTaxExemption taxExemption1 = new TblTaxExemption
                            {
                                ContactId = res.ContactId,
                                ExemptionCertificate = arrayString,
                                EffectiveDate = model.TaxExemptionVm?.EffectiveDate,
                                ExpiryDate = model.TaxExemptionVm?.ExpiryDate,
                                CreatedAt = DateTime.Now,
                                IsActive = true,
                                IsDelete = false,
                            };
                            await taxExemptionService.Add(taxExemption1);
                        }
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
                    responseVm.Message = AppConstant.TenantNotFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
        public async Task<IResponseVm<string>> Update_Contact(UpdateContact model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenantExists = await tenantService.AnyAsync(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true);

                if (IsTenantExists)
                {
                    var contact = await GetAllQuerable()
                        .FirstOrDefaultAsync(c => c.ContactId == model.ContactId && c.TenantId == AppConstant.LoggedInTenantId
                        && c.IsDelete == false);
                    var IsContactExists = GetAllQuerable()
                        .Where(c => (c.Email == model.Email || c.PhoneNumber == model.PhoneNumber)
                        && c.TenantId == AppConstant.LoggedInTenantId
                        && c.IsDelete == false && c.ContactId!=model.ContactId).ToList();
                    if (IsContactExists.Count > 0)
                    {
                        responseVm.Message = "Email or Phone " + AppConstant.AlreadyExists;
                        return responseVm;
                    }

                    if (model.TaxExemptionVm != null)
                    {
                        if (model.TaxExemptionVm.ExpiryDate < model.TaxExemptionVm.EffectiveDate)
                        {
                            responseVm.Message = "ExpiryDate must be greater than EffectiveDate.";
                            return responseVm;
                        }
                    }
                    if (contact != null)
                    {
                        contact.ContactType = model.ContactType;
                        contact.Name = model.Name;
                        contact.PhoneNumber = model.PhoneNumber;
                        contact.Email = model.Email;
                        contact.Website = model.Website;
                        contact.Notes = model.Notes;
                        contact.Referral = model.Referral;
                        contact.IsTaxExemption = model.IsTaxExemption;
                        contact.IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress;
                        contact.ModifiedBy = AppConstant.LoggedInUserID;
                        contact.UpdatedAt = DateTime.Now;

                        await Update(contact);
                        if (!model.IsBillingAddressSameAsAddress)
                        {
                            var contactBillingAddress = billingService.GetAllQuerable()
                                .Where(x => x.ContactId == contact.ContactId && x.IsDelete == false).FirstOrDefault();
                            if (contactBillingAddress != null)
                            {
                                contactBillingAddress.FullName = model.BillingAddressVm?.FullName;
                                contactBillingAddress.PhoneNumber = model.BillingAddressVm?.PhoneNumber;
                                contactBillingAddress.Email = model.BillingAddressVm?.Email;
                                contactBillingAddress.City = model.BillingAddressVm?.City;
                                contactBillingAddress.State = model.BillingAddressVm?.State;
                                contactBillingAddress.ZipCode = model.BillingAddressVm?.ZipCode;
                                contactBillingAddress.StreetAddress = model.BillingAddressVm?.StreetAddress;
                                contactBillingAddress.UpdatedAt = DateTime.Now;

                                await billingService.Update(contactBillingAddress);
                            }
                            else
                            {
                                TblContactBillingAddress CreateContactBillingAddress = new TblContactBillingAddress
                                {
                                    ContactId = contact.ContactId,
                                    FullName = model.BillingAddressVm?.FullName,
                                    PhoneNumber = model.BillingAddressVm?.PhoneNumber,
                                    Email = model.BillingAddressVm?.Email,
                                    City = model.BillingAddressVm?.City,
                                    State = model.BillingAddressVm?.State,
                                    ZipCode = model.BillingAddressVm?.ZipCode,
                                    StreetAddress = model.BillingAddressVm?.StreetAddress,
                                    CreatedAt = DateTime.Now,
                                    IsActive = true,
                                    IsDelete = false
                                };
                                await billingService.Add(CreateContactBillingAddress);
                            }
                        }

                        if (!model.IsTaxExemption)
                        {
                            var contactTaxExemption = taxExemptionService.GetAllQuerable()
                                .Where(x => x.ContactId == contact.ContactId && x.IsDelete == false).FirstOrDefault();
                            string arrayString = string.Join(", ", model.TaxExemptionVm?.ExemptionCertificate);

                            if (contactTaxExemption != null)
                            {
                                contactTaxExemption.ExemptionCertificate =arrayString;
                                contactTaxExemption.EffectiveDate = model.TaxExemptionVm?.EffectiveDate;
                                contactTaxExemption.ExpiryDate = model.TaxExemptionVm?.ExpiryDate;
                                contactTaxExemption.UpdatedAt = DateTime.Now;
                                contactTaxExemption.IsActive = true;
                                contactTaxExemption.IsDelete = false;
                                await taxExemptionService.Update(contactTaxExemption);
                            }
                            else
                            {
                                TblTaxExemption taxExemption1 = new TblTaxExemption
                                {
                                    ContactId = contact.ContactId,
                                    ExemptionCertificate = arrayString,
                                    EffectiveDate = model.TaxExemptionVm?.EffectiveDate,
                                    ExpiryDate = model.TaxExemptionVm?.ExpiryDate,
                                    CreatedAt = DateTime.Now,
                                    IsActive = true,
                                    IsDelete = false

                                };
                                await taxExemptionService.Add(taxExemption1);
                            }
                        }
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
                    responseVm.Message = AppConstant.TenantNotFound;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
        public async Task<IResponseVm<string>> ActiveInActiveContact(Guid ContactId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var contact = GetAllQuerable()
                    .Where(c => c.ContactId == ContactId
                    && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false).FirstOrDefault();
                if (contact == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }
                contact.IsActive = !contact.IsActive;
                contact.UpdatedAt = DateTime.Now;
                contact.ModifiedBy = AppConstant.LoggedInUserID;
                await Update(contact);

                var billingaddress = (await billingService.GetAll())
                   .Where(c => c.ContactId == ContactId && c.IsDelete == false).FirstOrDefault();

                if (billingaddress != null)
                {
                    billingaddress.IsActive = !billingaddress.IsActive;
                    billingaddress.UpdatedAt = DateTime.Now;
                    await billingService.Update(billingaddress);
                }
                var taxExemption = (await taxExemptionService.GetAll())
                    .Where(c => c.ContactId == ContactId && c.IsDelete == false)
                    .FirstOrDefault();

                if (taxExemption != null)
                {
                    taxExemption.IsActive = !taxExemption.IsActive;
                    taxExemption.UpdatedAt = DateTime.Now;
                    await taxExemptionService.Update(taxExemption);
                }
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
        public async Task<IResponseVm<string>> DeleteContact(Guid ContactId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var contact = GetAllQuerable()
                    .Where(c => c.ContactId == ContactId
                    && c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false).FirstOrDefault();
                if (contact == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }
                contact.IsActive = false;
                contact.IsDelete = true;
                contact.UpdatedAt = DateTime.Now;
                contact.ModifiedBy = AppConstant.LoggedInUserID;
                await Update(contact);

                var billingaddress = (await billingService.GetAll())
                   .Where(c => c.ContactId == ContactId && c.IsDelete == false).FirstOrDefault();

                if (billingaddress != null)
                {
                    billingaddress.IsActive = false;
                    billingaddress.IsDelete = true;
                    billingaddress.UpdatedAt = DateTime.Now;
                    await billingService.Update(billingaddress);
                }
                var taxExemption = (await taxExemptionService.GetAll())
                    .Where(c => c.ContactId == ContactId && c.IsDelete == false)
                    .FirstOrDefault();

                if (taxExemption != null)
                {
                    taxExemption.IsActive = false;
                    taxExemption.IsDelete = true;
                    taxExemption.UpdatedAt = DateTime.Now;
                    await taxExemptionService.Update(taxExemption);
                }
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.DeleteSuccessMsg;

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<dynamic>> GetAllArchiveContacts(ArchiveUser archiveModel)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
                var contact = GetAllQuerable()
                    .Where(c =>
                    (archiveModel.Status  ? c.IsActive == true : c.IsActive == false) &&
                    //(archiveModel.LastOrderDate != null ? c)
                     c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false ).ToList();
                if (contact == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }
                
                //contact.IsArchive = true;
                //contact.UpdatedAt = DateTime.Now;
                //contact.ModifiedBy = AppConstant.LoggedInUserID;
                //await Update(contact);
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.FetchSuccessMsg;
                responseVm.Data = contact;

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> ArchiveContacts(ArchiveUser archiveModel)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var contact = GetAllQuerable()
                    .Where(c =>
                    (archiveModel.Status ? c.IsActive == true : c.IsActive == false) &&
                    //(archiveModel.LastOrderDate != null ? c)
                     c.TenantId == AppConstant.LoggedInTenantId && c.IsDelete == false).ToList();
                if (contact == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }
                foreach (var c in contact) {
                    c.IsArchive = true;
                    await Update(c);
                }
               
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.UpdateSuccessMsg;

            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<List<ContactsList>>> GetAll_Contact(Pagination filter)
        {
            IResponseVm<List<ContactsList>> responseVm = new ResponseVm<List<ContactsList>>();
            try
            {
                var Contacts = new List<TblContact>();
                List<ContactsList> ContactList = new List<ContactsList>();
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
                        Contacts = (await GetAll()).Where(x => x.IsDelete == false && x.IsArchive == false && x.TenantId.ToString() == filter.TenantId).OrderByDescending(x=>x.CreatedAt).ToList();
                    }
                }
                else
                {
                    Contacts = (await GetAll()).Where(x => x.IsDelete == false && x.IsArchive == false && x.TenantId == AppConstant.LoggedInTenantId).OrderByDescending(x => x.CreatedAt).ToList();
                }


                foreach (var contact in Contacts)
                {
                    ContactsList contacts = new ContactsList
                    {
                        ContactId = contact.ContactId,
                        ContactType = contact.ContactType ?? "",
                        Email = contact.Email,
                        PhoneNumber = contact.PhoneNumber,
                        Name = contact.Name,
                        Website = contact.Website,
                        Notes = contact.Notes,
                        Referral = contact.Referral,
                        ContactUniqueId = contact.ContactUniqueId,
                        IsTaxExemption = contact.IsTaxExemption ?? false,
                        IsBillingAddressSameAsAddress = contact.IsBillingAddressSameAsAddress,
                        IsActive = contact.IsActive
                    };

                    contacts.billingAddressVm = billingService.GetAllQuerable()
                        .Where(x => x.ContactId == contact.ContactId
                        && x.IsDelete == false).Select(x=> new BillingAddressVm
                        {
                            City = x.City,
                            Email = x.Email,
                            FullName = x.FullName,
                            PhoneNumber = x.PhoneNumber,
                            State = x.State,
                            StreetAddress = x.StreetAddress,
                            ZipCode = x.ZipCode,
                        }).FirstOrDefault();
                    contacts.taxExemptionVm = taxExemptionService.GetAllQuerable()
                        .Where(x => x.ContactId == contact.ContactId
                        && x.IsDelete == false).Select(x => new TaxExemption
                        {
                            EffectiveDate = x.EffectiveDate,
                            ExemptionCertificate = x.ExemptionCertificate.Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray(),
                            ExpiryDate = x.ExpiryDate
                        }).FirstOrDefault();
                    ContactList.Add(contacts);
                }
                filter.SearchValue = filter.SearchValue.Trim().ToLower();
                var FilterRecords = ContactList
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&
                                    (filter.SearchValue != "" ?
                                    ((x.Notes ?? "").Contains(filter.SearchValue)
                                    || (x.ContactId.ToString()).Contains(filter.SearchValue)
                                    || (x.Email ?? "").Contains(filter.SearchValue)
                                    || (x.Website ?? "").Contains(filter.SearchValue)
                                    || (x.PhoneNumber ?? "").Contains(filter.SearchValue)
                                    || (x.Name ?? "").Contains(filter.SearchValue)
                                    ) : true)).ToList();
                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();
                    responseVm.IsSuccess = true;

                if (TakenRecords != null && TakenRecords.Count > 0)
                {
                    responseVm.Data = TakenRecords.ToList();
                    responseVm.Message = AppConstant.FetchSuccessMsg;
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

    }
}
