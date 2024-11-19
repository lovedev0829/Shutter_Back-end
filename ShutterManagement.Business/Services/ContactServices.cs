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

namespace ShutterManagement.Business.Services
{
    public class ContactServices : BaseRepository<ShutterDbContext, TblContact>, IContactService
    {
        private readonly ShutterDbContext _context;
        private readonly IContactBillingService billingService;
        private readonly ITenantService tenantService;
        private readonly ITaxExemption taxExemptionService;
        private readonly ClaimsProperty claimsProperty;

        public ContactServices(ShutterDbContext context, IContactBillingService billingService,
            ITenantService tenantService, ITaxExemption taxExemptionService, ClaimsProperty claimsProperty)
        {
            _context = context;
            this.billingService = billingService;
            this.tenantService = tenantService;
            this.taxExemptionService = taxExemptionService;
            this.claimsProperty = claimsProperty;
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
                        && c.IsActive == true && c.IsDelete == false).ToList();
                    if (IsContactExists.Count > 0)
                    {
                        responseVm.Message = "Email or Phone " + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    if (!model.IsBillingAddressSameAsAddress && model.BillingAddressVm == null)
                    {
                        responseVm.Message = "Billing details required.";
                        return responseVm;
                    }

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
                        IsTaxExemption = model.IsTaxExemption,
                        IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress,
                        CreatedBy = AppConstant.LoggedInUserID,
                        CreatedAt = DateTime.Now,
                        TenantId = AppConstant.LoggedInTenantId,
                        IsActive = true,
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
                            TblTaxExemption taxExemption1 = new TblTaxExemption
                            {
                                ContactId = res.ContactId,
                                ExemptionCertificate = model.TaxExemptionVm?.ExemptionCertificate,
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
                        && c.IsActive == true && c.IsDelete == false);

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
                                .Where(x => x.ContactId == contact.ContactId).FirstOrDefault();
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
                                };
                                await billingService.Add(CreateContactBillingAddress);
                            }
                        }

                        if (!model.IsTaxExemption)
                        {
                            var contactTaxExemption = taxExemptionService.GetAllQuerable()
                                .Where(x => x.ContactId == contact.ContactId).FirstOrDefault();
                            if (contactTaxExemption != null)
                            {
                                contactTaxExemption.ExemptionCertificate = model.TaxExemptionVm?.ExemptionCertificate;
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
                                    ExemptionCertificate = model.TaxExemptionVm?.ExemptionCertificate,
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
        public async Task<IResponseVm<string>> ActiveInActiveContact(Guid id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var contact = GetAllQuerable()
                    .Where(c => c.ContactId == id
                    && c.TenantId == AppConstant.LoggedInTenantId).FirstOrDefault();

                var billingaddress = (await billingService.GetAll())
                   .Where(c => c.ContactId == id && c.IsActive == true && c.IsDelete == false)
                   .ToList();

                if (billingaddress.Any())
                {
                    foreach (var billingaddress1 in billingaddress)
                    {
                        billingaddress1.IsActive = false;
                        await billingService.Update(billingaddress1);
                    }
                }
                var taxExemptionList = (await taxExemptionService.GetAll())
                    .Where(c => c.ContactId == id && c.IsActive == true && c.IsDelete == false)
                    .ToList();

                if (taxExemptionList.Any())
                {
                    foreach (var taxExemption in taxExemptionList)
                    {
                        taxExemption.IsActive = false;
                        taxExemption.UpdatedAt = DateTime.Now;
                        await taxExemptionService.Update(taxExemption);
                    }
                }

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
                if (contact == null)
                {
                    responseVm.Message = AppConstant.NoDataFound;
                    return responseVm;
                }
                var billingAddressList = (await billingService.GetAll())
                .Where(c => c.ContactId == id && c.IsActive == true && c.IsDelete == false)
                .ToList();

                if (billingAddressList.Any())
                {
                    foreach (var billingAddress in billingAddressList)
                    {
                        billingAddress.IsActive = false;
                        billingAddress.IsDelete = true;
                        await billingService.Update(billingAddress);
                    }
                }
                var taxExemptionVm = (await taxExemptionService.GetAll())
                    .Where(c => c.ContactId == id && c.IsActive == true && c.IsDelete == false)
                    .ToList();

                if (taxExemptionVm.Any())
                {
                    foreach (var taxExemption in taxExemptionVm)
                    {
                        taxExemption.IsActive = false;
                        taxExemption.IsDelete = true;
                        await taxExemptionService.Update(taxExemption);
                    }
                }

                contact.IsActive = false;
                contact.IsDelete = true;
                await Update(contact);

                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.DeleteSuccessMsg;

            }
            catch (Exception ex)
            {
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
                //foreach (var contact in ContactList) {
                //    contact.TblContactBillingAddresses = billingService.GetAllQuerable()
                //        .Where(x => x.ContactId == contact.ContactId && x.IsActive == true
                //        && x.IsDelete == false).FirstOrDefault();
                //}
                filter.SearchValue = filter.SearchValue.Trim().ToLower();
                var FilterRecords = ContactList
                                    .Where(x =>
                                    (filter.SearchValue != "" ?
                                    ((x.Notes ?? "").Contains(filter.SearchValue)
                                    || (x.Email ?? "").Contains(filter.SearchValue)
                                    || (x.Website ?? "").Contains(filter.SearchValue)
                                    || (x.PhoneNumber ?? "").Contains(filter.SearchValue)
                                    || (x.Name ?? "").Contains(filter.SearchValue)
                                    ) : true)).ToList();
                var TakenRecords = FilterRecords
                                    .Skip((filter.PageNo - 1) * filter.RecordPerPage).Take(filter.RecordPerPage).ToList();
                if (TakenRecords != null && TakenRecords.Count > 0)
                {
                    responseVm.Data = TakenRecords.Select(x => new
                    {
                        x.ContactId,
                        x.TenantId,
                        x.ContactType,
                        x.Email,
                        x.PhoneNumber,
                        x.Name,
                        x.Website,
                        x.Notes,
                        x.Referral,
                        x.IsTaxExemption,
                        x.IsBillingAddressSameAsAddress,
                    });
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



    }
}
