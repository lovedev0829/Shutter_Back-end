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

namespace ShutterManagement.Business.Services
{
    public class ContactServices : BaseRepository<ShutterDbContext, TblContact>, IContactService
    {
        private readonly ShutterDbContext _context;
        private readonly IContactBillingService billingService;
        private readonly ITenantService tenantService;
        private readonly ITaxExemption taxExemption;
        private readonly ClaimsProperty claimsProperty;

        public ContactServices(ShutterDbContext context, IContactBillingService billingService,
            ITenantService tenantService,ITaxExemption taxExemption, ClaimsProperty claimsProperty)
        {
            _context = context;
            this.billingService = billingService;
            this.tenantService = tenantService;
            this.taxExemption = taxExemption;
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
                    if (model.IsBillingAddressSameAsAddress && model.billingAddressVm == null) {
                        responseVm.Message = "Billing detaild required.";
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
                        IsBillingAddressSameAsAddress = model.IsBillingAddressSameAsAddress,
                        CreatedBy = AppConstant.LoggedInUserID,
                        CreatedAt = DateTime.Now
                    };
                    var res = await Add(CreateContact);
                    if (res.ContactId != Guid.Empty)
                    {
                        if (!model.IsBillingAddressSameAsAddress)
                        {

                            TblContactBillingAddress CreateContactBillingAddress = new TblContactBillingAddress
                            {
                                ContactId = res.ContactId,
                                Label = model.billingAddressVm?.Label,
                                AddressLine1 = model.billingAddressVm?.AddressLine1,
                                AddressLine2 = model.billingAddressVm?.AddressLine2,
                                City = model.billingAddressVm?.City,
                                State = model.billingAddressVm?.State,
                                ZipCode = model.billingAddressVm?.ZipCode,
                                Country = model.billingAddressVm?.Country,
                                CreatedAt = DateTime.Now,
                            };
                            await billingService.Add(CreateContactBillingAddress);
                        }
                        if(model.IstaxApplied != false)
                        {
                            TblTaxExemption taxExemption1 = new TblTaxExemption
                            {
                                ContactId = res.ContactId,
                                ExemptionCertificate = model.taxExemptionVm?.ExemptionCertificate,
                                EffectiveDate = model.taxExemptionVm?.EffectiveDate,
                                ExpiryDate = model.taxExemptionVm?.ExpiryDate,
                                CreatedBy = AppConstant.LoggedInUserID,
                                CreatedAt = DateTime.Now,
                                IsActive = true,
                                IsDelete = false

                            };
                            await taxExemption.Add(taxExemption1);
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
                                contactBillingAddress.Label = model.billingAddressVm?.Label;
                                contactBillingAddress.AddressLine1 = model.billingAddressVm?.AddressLine1;
                                contactBillingAddress.AddressLine2 = model.billingAddressVm?.AddressLine2;
                                contactBillingAddress.City = model.billingAddressVm?.City;
                                contactBillingAddress.State = model.billingAddressVm?.State;
                                contactBillingAddress.ZipCode = model.billingAddressVm?.ZipCode;
                                contactBillingAddress.Country = model.billingAddressVm?.Country;
                                contactBillingAddress.UpdatedAt = DateTime.Now;

                                await billingService.Update(contactBillingAddress);
                            }
                            else
                            {
                                TblContactBillingAddress CreateContactBillingAddress = new TblContactBillingAddress
                                {
                                    ContactId = contact.ContactId,
                                    Label = model.billingAddressVm?.Label,
                                    AddressLine1 = model.billingAddressVm?.AddressLine1,
                                    AddressLine2 = model.billingAddressVm?.AddressLine2,
                                    City = model.billingAddressVm?.City,
                                    State = model.billingAddressVm?.State,
                                    ZipCode = model.billingAddressVm?.ZipCode,
                                    Country = model.billingAddressVm?.Country,
                                    CreatedAt = DateTime.Now,
                                };
                                await billingService.Add(CreateContactBillingAddress);
                            }
                        }

                        if(model.IstaxApplied != false)
                        {
                            var contactTaxExemption = taxExemption.GetAllQuerable()
                                .Where(x => x.ContactId == contact.ContactId).FirstOrDefault();
                            if (contactTaxExemption != null)
                            {
                                contactTaxExemption.ExemptionCertificate = model.taxExemptionVm?.ExemptionCertificate;
                                contactTaxExemption.EffectiveDate = model.taxExemptionVm?.EffectiveDate;
                                contactTaxExemption.ExpiryDate = model.taxExemptionVm?.ExpiryDate;
                                contactTaxExemption.ModifiedBy = AppConstant.LoggedInUserID;
                                contactTaxExemption.UpdatedAt = DateTime.Now;
                                await taxExemption.Update(contactTaxExemption);
                            }
                            else
                            {
                                TblTaxExemption taxExemption1 = new TblTaxExemption
                                {
                                    ContactId = contact.ContactId,
                                    ExemptionCertificate = model.taxExemptionVm?.ExemptionCertificate,
                                    EffectiveDate = model.taxExemptionVm?.EffectiveDate,
                                    ExpiryDate = model.taxExemptionVm?.ExpiryDate,
                                    CreatedBy = AppConstant.LoggedInUserID,
                                    CreatedAt = DateTime.Now,
                                    IsActive = true,
                                    IsDelete = false

                                };
                                await taxExemption.Add(taxExemption1);
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
                var taxExemptionVm = (await taxExemption.GetAll())  
                    .Where(c => c.ContactId == id && c.IsActive == true && c.IsDelete == false)
                    .ToList();  

                if (taxExemptionVm.Any()) 
                {
                    foreach (var taxExemption2 in taxExemptionVm)
                    {
                        taxExemption2.IsActive = false; 
                        await taxExemption.Update(taxExemption2);    
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
                var billingaddress = (await billingService.GetAll())
                    .Where(c => c.ContactId == id && c.IsActive == true && c.IsDelete == false)
                    .ToList();

                if (billingaddress.Any())
                {
                    foreach (var billingaddress1 in billingaddress)
                    {
                        billingaddress1.IsActive = false;
                        billingaddress1.IsDelete = true;
                        await billingService.Update(billingaddress1);
                    }
                }
                var taxExemptionVm = (await taxExemption.GetAll())
                    .Where(c => c.ContactId == id && c.IsActive == true && c.IsDelete == false)
                    .ToList();

                if (taxExemptionVm.Any())
                {
                    foreach (var taxExemption2 in taxExemptionVm)
                    {
                        taxExemption2.IsActive = false;
                        taxExemption2.IsDelete = true;
                        await taxExemption.Update(taxExemption2);
                    }
                }
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
                        x.IsBillingAddressSameAsAddress,
                        //x.TblContactBillingAddresses
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
